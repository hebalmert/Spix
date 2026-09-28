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
using Spix.xNetwork.IpHelper;

namespace Spix.AppService.ImplementEntitiesNet;

//Las OLT de la corporacion: el equipo de central por donde entran los clientes de fibra.
//Es el gemelo de NodeService y sigue sus mismas reglas: toda consulta filtrada por la
//corporacion del usuario, usuario y clave solo para el Administrator, y una OLT con
//contratos no se borra.
//
//Spix NO se conecta a la OLT: la IP, el usuario y la clave solo quedan registrados.
public class OltService : IOltService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IIpControl _ipControl;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public OltService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IIpControl ipControl,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _ipControl = ipControl;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //OLT activas para elegir: solo id y nombre. Con id incluye la que ya tiene el contrato.
    public async Task<ActionResponse<IEnumerable<Olt>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Olt>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Olts
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && (x.Active || x.OltId == id))
                .OrderBy(x => x.OltName)
                .Select(x => new Olt { OltId = x.OltId, OltName = x.OltName })
                .ToListAsync();

            if (id == null)
            {
                list.Insert(0, new Olt
                {
                    OltId = Guid.Empty,
                    OltName = _localizer[nameof(Resource.Select_Olt)]
                });
            }

            return Success<IEnumerable<Olt>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Olt>>(ex);
        }
    }

    //El tablero: OLT activas e inactivas y cuantos contratos entran por ellas
    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<NetSummaryDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Una sola pasada por el indice de la corporacion para los tres conteos
            var summary = await _context.Olts
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .GroupBy(x => 1)
                .Select(g => new NetSummaryDto
                {
                    Total = g.Count(),
                    Active = g.Count(x => x.Active),
                    Inactive = g.Count(x => !x.Active)
                })
                .FirstOrDefaultAsync() ?? new NetSummaryDto();

            summary.Clients = await _context.ContractOlts.CountAsync(x => x.Olt!.CorporationId == corporationId);

            return Success(summary);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<NetSummaryDto>(ex);
        }
    }

    //El listado: por zona y nombre, sin credenciales
    public async Task<ActionResponse<IEnumerable<OltListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<OltListItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.Olts
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            //Se busca por nombre, zona o IP
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.OltName, $"%{filter}%") ||
                    EF.Functions.Like(x.Zone!.ZoneName, $"%{filter}%") ||
                    EF.Functions.Like(x.IpNetwork!.Ip!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.Zone!.ZoneName)
                .ThenBy(x => x.OltName)
                .Paginate(pagination)
                .Select(x => new OltListItemDto
                {
                    OltId = x.OltId,
                    OltName = x.OltName,
                    MarkName = x.Mark!.MarkName,
                    ZoneName = x.Zone!.ZoneName,
                    Ip = x.IpNetwork!.Ip,
                    PortCount = x.PortCount,
                    PortSpeed = x.PortSpeed,
                    Active = x.Active,
                    Clients = _context.ContractOlts.Count(c => c.OltId == x.OltId),
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                })
                .ToListAsync();

            return Success<IEnumerable<OltListItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<OltListItemDto>>(ex);
        }
    }

    //Para editar. Las credenciales solo van si quien pide es Administrator.
    public async Task<ActionResponse<Olt>> GetAsync(Guid id, string username, bool withCredentials)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<Olt>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.Olts
                .AsNoTracking()
                .Include(x => x.Zone)
                .FirstOrDefaultAsync(x => x.OltId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<Olt>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //El formulario elige departamento y ciudad a partir de la zona
            modelo.StateId = modelo.Zone!.StateId;
            modelo.CityId = modelo.Zone.CityId;
            modelo.Zone = null;

            if (!withCredentials)
            {
                modelo.Clave = string.Empty;
            }

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Olt>(ex);
        }
    }

    public async Task<ActionResponse<Olt>> AddAsync(Olt modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Olt>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            if (string.IsNullOrWhiteSpace(modelo.Clave)) return await FailRollbackAsync<Olt>(_localizer["Net_PasswordRequired"]);

            var name = modelo.OltName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, null)) return await FailRollbackAsync<Olt>(_localizer["Olt_NameRepeated", name]);

            //La IP tiene que ser de la corporacion y estar libre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, null, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Olt>(_localizer["Net_IpNotAvailable"]);

            //Lo que decide el servidor
            var nuevo = new Olt { CorporationId = corporationId.Value };
            CopyFields(modelo, nuevo, true);

            //Persistencia
            _context.Olts.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            nuevo.Clave = string.Empty;
            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Olt>(ex);
        }
    }

    //La clave vacia deja la que ya tenia (el Auxiliar no la recibe, asi que no la manda)
    public async Task<ActionResponse<Olt>> UpdateAsync(Olt modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Olt>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Olts.FirstOrDefaultAsync(x =>
                x.OltId == modelo.OltId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<Olt>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var name = modelo.OltName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, current.OltId)) return await FailRollbackAsync<Olt>(_localizer["Olt_NameRepeated", name]);

            //Si cambio la IP se suelta la anterior; si es la misma, solo se actualiza el nombre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, current.IpNetworkId, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Olt>(_localizer["Net_IpNotAvailable"]);

            //Mapeo campo por campo
            CopyFields(modelo, current, false);

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(new Olt { OltId = current.OltId, OltName = current.OltName });
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Olt>(ex);
        }
    }

    //Solo se borra una OLT sin contratos; su IP queda libre
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Olts.FirstOrDefaultAsync(x =>
                x.OltId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var inUse = await _context.ContractOlts.AnyAsync(x => x.OltId == id);
            if (inUse) return await FailRollbackAsync<bool>(_localizer["Olt_InUse", current.OltName]);

            //Persistencia
            await _ipControl.ReleaseAsync(current.IpNetworkId, corporationId.Value);
            _context.Olts.Remove(current);
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

    //Los datos que el usuario puede cambiar. La clave vacia no pisa la guardada.
    private static void CopyFields(Olt from, Olt to, bool isNew)
    {
        to.OltName = from.OltName.Trim();
        to.IpNetworkId = from.IpNetworkId;
        to.MarkId = from.MarkId;
        to.MarkModelId = from.MarkModelId;
        to.Usuario = from.Usuario;
        to.ZoneId = from.ZoneId;
        to.Latitude = from.Latitude;
        to.Longitude = from.Longitude;
        to.PortCount = from.PortCount;
        to.PortSpeed = from.PortSpeed;
        to.Active = from.Active;

        if (isNew || !string.IsNullOrWhiteSpace(from.Clave)) to.Clave = from.Clave;
    }

    private async Task<bool> NameExistsAsync(string name, int corporationId, Guid? oltId)
    {
        return await _context.Olts.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.OltName == name &&
            x.OltId != oltId);
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
