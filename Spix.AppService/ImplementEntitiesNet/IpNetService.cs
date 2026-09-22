using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using System.Net;

namespace Spix.AppService.ImplementEntitiesNet;

//Las IP que se asignan a los clientes (contratos).
//Toda consulta va filtrada por la corporacion del usuario. "Assigned" es estado del sistema:
//lo marcan los modulos que usan la IP, nunca el formulario. Una IP en uso no se borra ni
//cambia de direccion.
public class IpNetService : IIpNetService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public IpNetService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //IP libres para elegir. Sin id: con el neutro traducido. Con id: incluye la IP que ya tiene
    //el registro que se esta editando, para que el combo la muestre.
    public async Task<ActionResponse<IEnumerable<IpNet>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<IpNet>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.IpNets
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId &&
                            ((x.Active && !x.Assigned && !x.Excluded) || x.IpNetId == id))
                .OrderBy(x => x.IpSort)
                .ToListAsync();

            if (id == null)
            {
                list.Insert(0, new IpNet
                {
                    IpNetId = Guid.Empty,
                    Ip = _localizer[nameof(Resource.Select_IP)]
                });
            }

            return Success<IEnumerable<IpNet>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNet>>(ex);
        }
    }

    //El tablero: cuantas hay, cuantas libres, asignadas y excluidas, sobre todo el listado
    public async Task<ActionResponse<IpSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IpSummaryDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Una sola pasada por el indice de la corporacion, con los cuatro conteos a la vez
            var summary = await _context.IpNets
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .GroupBy(x => 1)
                .Select(g => new IpSummaryDto
                {
                    Total = g.Count(),
                    Free = g.Count(x => x.Active && !x.Assigned && !x.Excluded),
                    Assigned = g.Count(x => x.Assigned),
                    Excluded = g.Count(x => x.Excluded)
                })
                .FirstOrDefaultAsync();

            return Success(summary ?? new IpSummaryDto());
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IpSummaryDto>(ex);
        }
    }

    //El listado va en orden numerico de la IP (10.0.0.2 antes que 10.0.0.10)
    public async Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<IpNet>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.IpNets
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Ip!, $"%{filter}%") ||
                    EF.Functions.Like(x.Description!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.IpSort)
                .Paginate(pagination)
                .ToListAsync();

            return Success<IEnumerable<IpNet>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNet>>(ex);
        }
    }

    public async Task<ActionResponse<IpNet>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IpNet>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<IpNet>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex);
        }
    }

    public async Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<IpNet>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            if (!IsValidIp(modelo.Ip)) return await FailRollbackAsync<IpNet>(_localizer["Ip_InvalidFormat", modelo.Ip ?? string.Empty]);
            if (await IpExistsAsync(modelo.Ip!, corporationId.Value, null)) return await FailRollbackAsync<IpNet>(_localizer["Ip_Repeated", modelo.Ip!]);

            //Lo que decide el servidor: una IP nueva nunca nace asignada
            var nuevo = new IpNet
            {
                Ip = modelo.Ip,
                Description = modelo.Description,
                Active = modelo.Active,
                Assigned = false,
                Excluded = modelo.Excluded,
                CorporationId = corporationId.Value
            };

            //Persistencia
            _context.IpNets.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex);
        }
    }

    //Se editan la descripcion y los estados Activa/Excluida; la direccion, solo si la IP no esta en uso
    public async Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<IpNet>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.IpNets.FirstOrDefaultAsync(x =>
                x.IpNetId == modelo.IpNetId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<IpNet>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (!IsValidIp(modelo.Ip)) return await FailRollbackAsync<IpNet>(_localizer["Ip_InvalidFormat", modelo.Ip ?? string.Empty]);

            if (modelo.Ip != current.Ip)
            {
                if (await IsInUseAsync(current)) return await FailRollbackAsync<IpNet>(_localizer["Ip_InUseChange", current.Ip ?? string.Empty]);
                if (await IpExistsAsync(modelo.Ip!, corporationId.Value, current.IpNetId)) return await FailRollbackAsync<IpNet>(_localizer["Ip_Repeated", modelo.Ip!]);
            }

            //Mapeo campo por campo: Assigned no se toca, es estado del sistema
            current.Ip = modelo.Ip;
            current.Description = modelo.Description;
            current.Active = modelo.Active;
            current.Excluded = modelo.Excluded;

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNet>(ex);
        }
    }

    //Carga un rango base.desde..base.hasta; las que ya existen se saltan
    public async Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username)
    {
        var poolError = ValidatePool(modelo);
        if (poolError != null) return Fail<int>(poolError);

        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<int>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var requestedIps = PoolIps(modelo);
            var existing = await _context.IpNets
                .Where(x => x.CorporationId == corporationId && requestedIps.Contains(x.Ip!))
                .Select(x => x.Ip!)
                .ToListAsync();
            var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var nuevas = requestedIps
                .Where(ip => !existingSet.Contains(ip))
                .Select(ip => new IpNet
                {
                    Ip = ip,
                    Active = true,
                    Assigned = false,
                    Excluded = false,
                    CorporationId = corporationId.Value
                })
                .ToList();

            //Persistencia
            if (nuevas.Count > 0)
            {
                _context.IpNets.AddRange(nuevas);
                await _transactionManager.SaveChangesAsync();
            }
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevas.Count);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<int>(ex);
        }
    }

    //Borra el rango, pero solo las IP libres: las asignadas, excluidas o usadas se quedan
    public async Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username)
    {
        var poolError = ValidatePool(modelo);
        if (poolError != null) return Fail<int>(poolError);

        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<int>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var requestedIps = PoolIps(modelo);
            var libres = await _context.IpNets
                .Where(x => x.CorporationId == corporationId &&
                            requestedIps.Contains(x.Ip!) &&
                            !x.Assigned &&
                            !x.Excluded &&
                            !x.ContractIps!.Any() && !x.ContractQues!.Any() && !x.ContractBinds!.Any())
                .ToListAsync();

            //Persistencia
            if (libres.Count > 0)
            {
                _context.IpNets.RemoveRange(libres);
                await _transactionManager.SaveChangesAsync();
            }
            await _transactionManager.CommitTransactionAsync();

            return Success(libres.Count);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<int>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.IpNets.FirstOrDefaultAsync(x =>
                x.IpNetId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (await IsInUseAsync(current)) return await FailRollbackAsync<bool>(_localizer["Ip_InUseDelete", current.Ip ?? string.Empty]);

            //Persistencia
            _context.IpNets.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //En uso: marcada como asignada o referenciada por un contrato (IP, Queue o IpBinding)
    private async Task<bool> IsInUseAsync(IpNet ip)
    {
        if (ip.Assigned) return true;

        return await _context.IpNets
            .Where(x => x.IpNetId == ip.IpNetId)
            .AnyAsync(x => !(!x.ContractIps!.Any() && !x.ContractQues!.Any() && !x.ContractBinds!.Any()));
    }

    private async Task<bool> IpExistsAsync(string ip, int corporationId, Guid? currentId)
    {
        return await _context.IpNets.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.Ip == ip &&
            x.IpNetId != currentId);
    }

    //Solo IPv4 con sus cuatro partes: la entidad ya limpia lo que escribe el usuario
    private static bool IsValidIp(string? ip)
    {
        return !string.IsNullOrWhiteSpace(ip) &&
               ip.Split('.').Length == 4 &&
               IPAddress.TryParse(ip, out var parsed) &&
               parsed.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    //Base de tres partes y un rango dentro de 0..255
    private string? ValidatePool(IpNetPoolCreateDTO modelo)
    {
        var ipBase = modelo.IpAddress?.Trim();
        if (string.IsNullOrWhiteSpace(ipBase) || ipBase.Split('.').Length != 3 || !IsValidIp($"{ipBase}.0"))
            return _localizer["Ip_InvalidPoolBase"];

        if (modelo.Desde < 0 || modelo.Hasta > 255 || modelo.Desde > modelo.Hasta)
            return _localizer["Ip_InvalidPoolRange"];

        return null;
    }

    private static List<string> PoolIps(IpNetPoolCreateDTO modelo)
    {
        var ipBase = modelo.IpAddress!.Trim();
        return Enumerable
            .Range(modelo.Desde, modelo.Hasta - modelo.Desde + 1)
            .Select(number => $"{ipBase}.{number}")
            .ToList();
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
