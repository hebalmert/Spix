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

//Las IP de la red: nodos, servidores y equipos.
//Toda consulta va filtrada por la corporacion del usuario. "Assigned" es estado del sistema:
//lo marcan los modulos que usan la IP, nunca el formulario. Una IP en uso no se borra ni
//cambia de direccion.
public class IpNetworkService : IIpNetworkService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public IpNetworkService(
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
    //La lista para la IP local del PPPoE de UN servidor.
    //
    //Devuelve TODAS las IP de red activas de la corporacion, sin esconder ninguna: en
    //MikroTik el local-address no tiene que existir en una interfaz, y el caso mas comun
    //es justo la IP de gestion del propio equipo.
    //
    //Las que YA tiene otro equipo vienen marcadas en Description con el nombre de ese
    //equipo. No se bloquean: el front avisa de quien es y deja decidir. Esta lista NO
    //toca nada en el modulo de IP de red.
    public async Task<ActionResponse<IEnumerable<IpNetwork>>> ComboLocalPppAsync(string username, Guid serverId)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<IpNetwork>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //El servidor tiene que ser de SU corporacion: de aqui salen las dos IPs que
            //se admiten por fuera del pozo libre.
            var server = await _context.Servers
                .AsNoTracking()
                .Where(x => x.ServerId == serverId && x.CorporationId == corporationId)
                .Select(x => new { x.IpNetworkId, x.PppLocalIpNetId })
                .FirstOrDefaultAsync();

            if (server == null) return Fail<IEnumerable<IpNetwork>>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //De quien es cada IP de red: su equipo de gestion, o el que la use como IP
            //local de PPPoE. Se resuelve de una pasada, no una consulta por fila.
            var duenos = await _context.Servers
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.ServerId != serverId)
                .Select(x => new { x.ServerName, x.IpNetworkId, x.PppLocalIpNetId })
                .ToListAsync();

            var list = await _context.IpNetworks
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active)
                .OrderBy(x => x.IpSort)
                .ToListAsync();

            foreach (var ip in list)
            {
                var dueno = duenos.FirstOrDefault(d => d.IpNetworkId == ip.IpNetworkId ||
                                                       d.PppLocalIpNetId == ip.IpNetworkId);

                //Description viaja solo para avisar: si esta vacia, la IP esta libre
                ip.Description = dueno?.ServerName;
            }

            //El neutro va SIEMPRE: es la unica forma de dejar el campo en blanco. Si solo
            //se pone cuando no hay una elegida, una vez elegida no se puede quitar nunca.
            list.Insert(0, new IpNetwork
            {
                IpNetworkId = Guid.Empty,
                Ip = _localizer[nameof(Resource.Select_IP)]
            });

            return Success<IEnumerable<IpNetwork>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNetwork>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<IpNetwork>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.IpNetworks
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId &&
                            ((x.Active && !x.Assigned && !x.Excluded) || x.IpNetworkId == id))
                .OrderBy(x => x.IpSort)
                .ToListAsync();

            if (id == null)
            {
                list.Insert(0, new IpNetwork
                {
                    IpNetworkId = Guid.Empty,
                    Ip = _localizer[nameof(Resource.Select_IP)]
                });
            }

            return Success<IEnumerable<IpNetwork>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNetwork>>(ex);
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
            var summary = await _context.IpNetworks
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
    public async Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<IpNetwork>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.IpNetworks
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

            return Success<IEnumerable<IpNetwork>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IpNetwork>>(ex);
        }
    }

    public async Task<ActionResponse<IpNetwork>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IpNetwork>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.IpNetworks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetworkId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<IpNetwork>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex);
        }
    }

    public async Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<IpNetwork>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            if (!IsValidIp(modelo.Ip)) return await FailRollbackAsync<IpNetwork>(_localizer["Ip_InvalidFormat", modelo.Ip ?? string.Empty]);
            if (await IpExistsAsync(modelo.Ip!, corporationId.Value, null)) return await FailRollbackAsync<IpNetwork>(_localizer["Ip_Repeated", modelo.Ip!]);

            //Lo que decide el servidor: una IP nueva nunca nace asignada
            var nuevo = new IpNetwork
            {
                Ip = modelo.Ip,
                Description = modelo.Description,
                Active = modelo.Active,
                Assigned = false,
                Excluded = modelo.Excluded,
                CorporationId = corporationId.Value
            };

            //Persistencia
            _context.IpNetworks.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex);
        }
    }

    //Se editan la descripcion y los estados Activa/Excluida; la direccion, solo si la IP no esta en uso
    public async Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<IpNetwork>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.IpNetworks.FirstOrDefaultAsync(x =>
                x.IpNetworkId == modelo.IpNetworkId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<IpNetwork>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (!IsValidIp(modelo.Ip)) return await FailRollbackAsync<IpNetwork>(_localizer["Ip_InvalidFormat", modelo.Ip ?? string.Empty]);

            if (modelo.Ip != current.Ip)
            {
                if (await IsInUseAsync(current)) return await FailRollbackAsync<IpNetwork>(_localizer["Ip_InUseChange", current.Ip ?? string.Empty]);
                if (await IpExistsAsync(modelo.Ip!, corporationId.Value, current.IpNetworkId)) return await FailRollbackAsync<IpNetwork>(_localizer["Ip_Repeated", modelo.Ip!]);
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
            return await _httpErrorHandler.HandleErrorAsync<IpNetwork>(ex);
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
            var existing = await _context.IpNetworks
                .Where(x => x.CorporationId == corporationId && requestedIps.Contains(x.Ip!))
                .Select(x => x.Ip!)
                .ToListAsync();
            var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var nuevas = requestedIps
                .Where(ip => !existingSet.Contains(ip))
                .Select(ip => new IpNetwork
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
                _context.IpNetworks.AddRange(nuevas);
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
            var libres = await _context.IpNetworks
                .Where(x => x.CorporationId == corporationId &&
                            requestedIps.Contains(x.Ip!) &&
                            !x.Assigned &&
                            !x.Excluded &&
                            !x.Nodes!.Any() && !x.Servers!.Any())
                .ToListAsync();

            //Persistencia
            if (libres.Count > 0)
            {
                _context.IpNetworks.RemoveRange(libres);
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

            var current = await _context.IpNetworks.FirstOrDefaultAsync(x =>
                x.IpNetworkId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (await IsInUseAsync(current)) return await FailRollbackAsync<bool>(_localizer["Ip_InUseDelete", current.Ip ?? string.Empty]);

            //Persistencia
            _context.IpNetworks.Remove(current);
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

    //En uso: marcada como asignada o referenciada por un nodo o un servidor
    private async Task<bool> IsInUseAsync(IpNetwork ip)
    {
        if (ip.Assigned) return true;

        return await _context.IpNetworks
            .Where(x => x.IpNetworkId == ip.IpNetworkId)
            .AnyAsync(x => !(!x.Nodes!.Any() && !x.Servers!.Any()));
    }

    private async Task<bool> IpExistsAsync(string ip, int corporationId, Guid? currentId)
    {
        return await _context.IpNetworks.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.Ip == ip &&
            x.IpNetworkId != currentId);
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
