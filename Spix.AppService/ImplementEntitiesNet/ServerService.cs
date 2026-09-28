using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using Spix.xNetwork.IpHelper;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementEntitiesNet;

//Los servidores (MikroTik) de la corporacion. Toda consulta va filtrada por la corporacion del usuario.
//Usuario y clave solo los recibe el Administrator: ni el listado ni el combo los llevan.
//Un servidor con contratos, colas o suspensiones no se borra: se inactiva.
public class ServerService : IServerService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IIpControl _ipControl;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ServerService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IIpControl ipControl,
        IEnumMultilLanguageService enumMultilLanguageService,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _ipControl = ipControl;
        _enumMultilLanguageService = enumMultilLanguageService;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    //Como puede trabajar un equipo. La lista se arma ACA, con los nombres traducidos y el
    //neutro en la posicion 0: el front solo pone value y @onchange.
    public ActionResponse<IEnumerable<IntItemModel>> ControlTypesCombo()
    {
        var list = _enumMultilLanguageService
            .GetEnumSelectList<MikrotikControlType>(nameof(Resource.Select_ControlType));

        return Success<IEnumerable<IntItemModel>>(list);
    }

    //Servidores activos para elegir: solo id y nombre. Con id incluye el que ya tiene el contrato.
    public async Task<ActionResponse<IEnumerable<Server>>> ComboAsync(string username, Guid? id = null)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Server>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Servers
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && (x.Active || x.ServerId == id))
                .OrderBy(x => x.ServerName)
                .Select(x => new Server { ServerId = x.ServerId, ServerName = x.ServerName, ControlMk = x.ControlMk })
                .ToListAsync();

            if (id == null)
            {
                list.Insert(0, new Server
                {
                    ServerId = Guid.Empty,
                    ServerName = _localizer[nameof(Resource.Select_Server)]
                });
            }

            return Success<IEnumerable<Server>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Server>>(ex);
        }
    }

    //El tablero: servidores activos e inactivos y cuantos contratos salen por ellos
    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<NetSummaryDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Una sola pasada por el indice de la corporacion para los tres conteos de equipos
            var summary = await _context.Servers
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

            //Los clientes se cuentan por el indice del equipo en la tabla de contratos
            summary.Clients = await _context.ContractServers.CountAsync(x => x.Server!.CorporationId == corporationId);

            return Success(summary);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<NetSummaryDto>(ex);
        }
    }

    //El listado, sin credenciales
    public async Task<ActionResponse<IEnumerable<ServerListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<ServerListItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.Servers
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            //Se busca por nombre, zona o IP
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ServerName, $"%{filter}%") ||
                    EF.Functions.Like(x.Zone!.ZoneName, $"%{filter}%") ||
                    EF.Functions.Like(x.IpNetwork!.Ip!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.ServerName)
                .Paginate(pagination)
                .Select(x => new ServerListItemDto
                {
                    ServerId = x.ServerId,
                    ServerName = x.ServerName,
                    ControlMk = x.ControlMk,
                    PppoeReady = x.PppServerMkId != null,
                    ZoneName = x.Zone!.ZoneName,
                    Ip = x.IpNetwork!.Ip,
                    Active = x.Active,
                    Clients = _context.ContractServers.Count(c => c.ServerId == x.ServerId)
                })
                .ToListAsync();

            return Success<IEnumerable<ServerListItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ServerListItemDto>>(ex);
        }
    }

    //Para editar. La clave solo va si quien pide es Administrator.
    public async Task<ActionResponse<Server>> GetAsync(Guid id, string username, bool withCredentials)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<Server>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.Servers
                .AsNoTracking()
                .Include(x => x.Zone)
                .FirstOrDefaultAsync(x => x.ServerId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<Server>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //El formulario elige departamento y ciudad a partir de la zona
            modelo.StateId = modelo.Zone!.StateId;
            modelo.CityId = modelo.Zone.CityId;
            modelo.Zone = null;

            if (!withCredentials) modelo.Clave = string.Empty;

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex);
        }
    }

    public async Task<ActionResponse<Server>> AddAsync(Server modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Server>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            if (string.IsNullOrWhiteSpace(modelo.Clave)) return await FailRollbackAsync<Server>(_localizer["Net_PasswordRequired"]);

            var name = modelo.ServerName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, null)) return await FailRollbackAsync<Server>(_localizer["Server_NameRepeated", name]);

            //La IP tiene que ser de la corporacion y estar libre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, null, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Server>(_localizer["Net_IpNotAvailable"]);

            //Lo que decide el servidor
            var nuevo = new Server { CorporationId = corporationId.Value };
            CopyFields(modelo, nuevo, true);

            //Persistencia
            _context.Servers.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            nuevo.Clave = string.Empty;
            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex);
        }
    }

    //La clave vacia deja la que ya tenia (el Auxiliar no la recibe, asi que no la manda)
    public async Task<ActionResponse<Server>> UpdateAsync(Server modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Server>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Servers.FirstOrDefaultAsync(x =>
                x.ServerId == modelo.ServerId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<Server>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var name = modelo.ServerName.Trim();
            if (await NameExistsAsync(name, corporationId.Value, current.ServerId)) return await FailRollbackAsync<Server>(_localizer["Server_NameRepeated", name]);

            //Un equipo ya alistado en el MikroTik no cambia de interfaz ni de IP local: el
            //servidor PPPoE y el perfil ya estan escritos con esos valores, asi que la base
            //mostraria una cosa y el router tendria otra.
            if (current.PppServerMkId != null &&
                (modelo.LanName != current.LanName || modelo.PppLocalIpNetId != current.PppLocalIpNetId))
            {
                return await FailRollbackAsync<Server>(_localizer["Server_PppoeLocked"]);
            }

            //La IP local del PPPoE tiene que ser de SU corporacion y estar activa. NO se
            //exige que este libre: el caso mas comun es la IP de gestion del PROPIO equipo,
            //que por definicion esta tomada por el. Exigir que estuviera libre dejaba fuera
            //justo el caso normal.
            //
            //Tampoco se marca como asignada al crear el servidor PPPoE: esta IP sale del
            //pozo de RED y los contratos sacan la suya del pozo de CLIENTES, asi que nunca
            //se le puede entregar a nadie.
            if (modelo.PppLocalIpNetId != null && modelo.PppLocalIpNetId != current.PppLocalIpNetId)
            {
                var ipOkLocal = await _context.IpNetworks.AnyAsync(x =>
                    x.IpNetworkId == modelo.PppLocalIpNetId &&
                    x.CorporationId == corporationId &&
                    x.Active);

                if (!ipOkLocal) return await FailRollbackAsync<Server>(_localizer["Net_IpNotAvailable"]);

                //Que la IP sea de otro equipo NO se bloquea aqui: el combo la muestra
                //diciendo de quien es y el operador confirma en pantalla. Si dijo que si,
                //el servidor no tiene por que volver a negarse.
            }

            //Cambiar como trabaja el equipo con clientes ya provisionados deja los espejos
            //huerfanos: ip-bindings o secrets que nadie volveria a tocar. Se bloquea.
            if (modelo.ControlMk != current.ControlMk)
            {
                var provisionado = await _context.ContractBinds.AnyAsync(x => x.ServerId == current.ServerId) ||
                                   await _context.ContractPppoes.AnyAsync(x => x.ServerId == current.ServerId) ||
                                   await _context.ContractQues.AnyAsync(x => x.ServerId == current.ServerId) ||
                                   current.PppServerMkId != null;

                if (provisionado) return await FailRollbackAsync<Server>(_localizer["Server_ControlMkInUse"]);
            }

            //Cambiar la IP de gestion de un equipo YA PROVISIONADO es legitimo: los routers
            //cambian de IP. Pero los MikrotikId guardados apuntan a filas de ESE equipo, asi
            //que hay que comprobar que del otro lado siga estando el mismo.
            if (modelo.IpNetworkId != current.IpNetworkId)
            {
                var provisionadoEnEquipo = await _context.ContractBinds.AnyAsync(x => x.ServerId == current.ServerId) ||
                                           await _context.ContractPppoes.AnyAsync(x => x.ServerId == current.ServerId) ||
                                           await _context.ContractQues.AnyAsync(x => x.ServerId == current.ServerId) ||
                                           current.PppServerMkId != null;

                if (provisionadoEnEquipo)
                {
                    //Sin identidad aprendida no hay con que comparar: que el operador pruebe
                    //la conexion primero, que es lo que la aprende.
                    if (string.IsNullOrWhiteSpace(current.MkIdentity))
                    {
                        return await FailRollbackAsync<Server>(_localizer["Server_IdentityUnknown"]);
                    }

                    var ipNueva = await _context.IpNetworks
                        .AsNoTracking()
                        .Where(x => x.IpNetworkId == modelo.IpNetworkId)
                        .Select(x => x.Ip)
                        .FirstOrDefaultAsync();

                    var identidadNueva = LeerIdentidad(ipNueva, modelo.ApiPort, modelo.Usuario,
                        string.IsNullOrWhiteSpace(modelo.Clave) ? current.Clave : modelo.Clave);

                    if (identidadNueva == null)
                    {
                        return await FailRollbackAsync<Server>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                    }

                    if (!string.Equals(identidadNueva, current.MkIdentity, StringComparison.OrdinalIgnoreCase))
                    {
                        return await FailRollbackAsync<Server>(_localizer["Server_IdentityMismatch", current.MkIdentity, identidadNueva]);
                    }
                }
            }

            //Si cambio la IP se suelta la anterior; si es la misma, solo se actualiza el nombre
            var ipOk = await _ipControl.AssignAsync(modelo.IpNetworkId, current.IpNetworkId, name, corporationId.Value);
            if (!ipOk) return await FailRollbackAsync<Server>(_localizer["Net_IpNotAvailable"]);

            //Mapeo campo por campo
            CopyFields(modelo, current, false);

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(new Server { ServerId = current.ServerId, ServerName = current.ServerName });
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Server>(ex);
        }
    }

    //Solo se borra un servidor que nadie usa; su IP queda libre
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Servers.FirstOrDefaultAsync(x =>
                x.ServerId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var inUse = await _context.ContractServers.AnyAsync(x => x.ServerId == id) ||
                        await _context.ContractBinds.AnyAsync(x => x.ServerId == id) ||
                        await _context.ContractPppoes.AnyAsync(x => x.ServerId == id) ||
                        await _context.ContractQues.AnyAsync(x => x.ServerId == id) ||
                        await _context.QueueParents.AnyAsync(x => x.ServerId == id) ||
                        await _context.ContractSuspendeds.AnyAsync(x => x.ServerId == id);
            if (inUse) return await FailRollbackAsync<bool>(_localizer["Server_InUse", current.ServerName]);

            //Y tampoco se borra un equipo que sigue teniendo su servidor PPPoE escrito en el
            //MikroTik: la fila desapareceria de la base y el router quedaria configurado, sin
            //nadie que sepa que ese perfil y ese servidor PPPoE son de Spix.
            if (current.PppServerMkId != null) return await FailRollbackAsync<bool>(_localizer["Server_PppoeProvisioned", current.ServerName]);

            //Persistencia
            await _ipControl.ReleaseAsync(current.IpNetworkId, corporationId.Value);
            _context.Servers.Remove(current);
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

    //Le pregunta al equipo como se llama. null si no se pudo hablar con el.
    //
    //Es la misma orden que usa la pantalla de probar conexion. Se lee aca y no por el
    //servicio de conexiones para no invertir la dependencia entre los dos.
    private static string? LeerIdentidad(string? ip, int puerto, string usuario, string clave)
    {
        if (string.IsNullOrWhiteSpace(ip)) return null;

        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(ip, puerto);

            if (!mikrotik.Login(usuario, clave)) return null;

            mikrotik.Send("/system/identity/print");
            mikrotik.Send("=.proplist=name", true);

            foreach (var sentence in mikrotik.Read())
            {
                if (sentence.StartsWith("!trap")) return null;

                if (!sentence.StartsWith("!re")) continue;

                var partes = sentence.Split('=');

                for (int i = 1; i + 1 < partes.Length; i += 2)
                {
                    if (partes[i].Equals("name", StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(partes[i + 1]))
                    {
                        return partes[i + 1];
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (mikrotik != null)
            {
                try
                {
                    mikrotik.Close();
                }
                catch
                {
                }
            }
        }
    }

    //Los datos que el usuario puede cambiar. La clave vacia no pisa la guardada.
    private static void CopyFields(Server from, Server to, bool isNew)
    {
        to.ServerName = from.ServerName.Trim();
        to.IpNetworkId = from.IpNetworkId;
        to.Usuario = from.Usuario;
        to.WanName = from.WanName;
        to.LanName = from.LanName;
        to.ControlMk = from.ControlMk;
        to.PppLocalIpNetId = from.PppLocalIpNetId;
        to.ApiPort = from.ApiPort;
        to.MarkId = from.MarkId;
        to.MarkModelId = from.MarkModelId;
        to.ZoneId = from.ZoneId;
        to.Active = from.Active;

        if (isNew || !string.IsNullOrWhiteSpace(from.Clave)) to.Clave = from.Clave;
    }

    private async Task<bool> NameExistsAsync(string name, int corporationId, Guid? serverId)
    {
        return await _context.Servers.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.ServerName == name &&
            x.ServerId != serverId);
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
