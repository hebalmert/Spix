using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

// Suspender y reactivar DESDE EL ESCRITORIO.
//
// Por que existe: el Backend le habla al MikroTik por IP publica; el escritorio le habla
// por la red LAN, que es la unica forma de atender a un cliente sin IP publica. Entonces
// el trabajo va partido: aqui se juntan los datos y se validan las reglas, el escritorio
// escribe el equipo, y despues vuelve aqui a guardar.
//
// Las reglas NO se mueven al escritorio: quedan de este lado, que es donde estan las que
// ya usa Blazor. Este servicio nunca abre una conexion al equipo.
public partial class ContractSuspendedMkService : IContractSuspendedMkService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public ContractSuspendedMkService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        IStringLocalizer localizer,
        HttpErrorHandler httpErrorHandler,
        IContractActivationIntegrityService contractActivationIntegrityService)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
        _contractActivationIntegrityService = contractActivationIntegrityService;
    }

    // Con que bindings hay que hablar y que ponerles. Las reglas de si se puede suspender
    // se revisan aqui: si alguna falla, viene Blocked y el escritorio no toca el equipo.
    public async Task<ActionResponse<SuspendMkSetupDTO>> GetSuspendSetupAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<SuspendMkSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new SuspendMkSetupDTO();

            var contract = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contract == null)
            {
                datos.Blocked = _localizer[nameof(Resource.Generic_IdNotFound)];
                return Ok(datos);
            }

            //1. Solo se suspende lo que esta activo
            if (contract.ContractState != ContractState.Active)
            {
                datos.Blocked = _localizer["Suspend_OnlyActive"];
                return Ok(datos);
            }

            datos.NombreCliente = $"{contract.Client?.FirstName} {contract.Client?.LastName} - ({contract.ControlContrato})";
            datos.UsaHotSpot = await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId);

            //Sin HotSpot no hay nada que escribir en el equipo: solo cambia el estado
            if (!datos.UsaHotSpot)
            {
                return Ok(datos);
            }

            //2. Con el acceso dando servicio: IpBinding presente y en bypassed
            var bind = await _context.ContractBinds
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId)
                .Select(x => new { x.HotSpotTypeId })
                .FirstOrDefaultAsync();

            var hotSpotBypassed = await _context.HotSpotTypes
                .AsNoTracking()
                .Where(x => x.Active && x.TypeName == "bypassed")
                .Select(x => x.HotSpotTypeId)
                .FirstOrDefaultAsync();

            if (bind == null || bind.HotSpotTypeId != hotSpotBypassed)
            {
                datos.Blocked = _localizer["Suspend_NeedsBypassed"];
                return Ok(datos);
            }

            //3. El tipo con el que queda: regular, o sea el cliente cae en el portal
            var regularType = await _context.HotSpotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");

            if (regularType == null)
            {
                datos.Blocked = _localizer["Suspend_NoRegularType"];
                return Ok(datos);
            }

            datos.TipoRegular = regularType.TypeName;

            //4. Los bindings, con su servidor: el escritorio abre una conexion por servidor
            var bindings = await _context.ContractBinds
                .AsNoTracking()
                .Include(x => x.Server!).ThenInclude(x => x.IpNetwork)
                .Include(x => x.IpNet)
                .Include(x => x.CargueDetail)
                .Where(x => x.ContractClientId == contractClientId)
                .ToListAsync();

            if (bindings.Count == 0)
            {
                datos.Blocked = _localizer["Suspend_NeedsBypassed"];
                return Ok(datos);
            }

            foreach (var binding in bindings)
            {
                if (binding.Server?.IpNetwork?.Ip == null ||
                    binding.IpNet?.Ip == null ||
                    binding.CargueDetail?.MacWlan == null ||
                    string.IsNullOrWhiteSpace(binding.MikrotikId))
                {
                    datos.Blocked = _localizer["Suspend_BindIncomplete"];
                    return Ok(datos);
                }

                datos.Bindings.Add(new SuspendBindingDTO
                {
                    ServerId = binding.ServerId,
                    ServerName = binding.Server!.ServerName,
                    ServerIp = binding.Server.IpNetwork!.Ip,
                    Usuario = binding.Server.Usuario,
                    Clave = binding.Server.Clave,
                    ApiPort = binding.Server.ApiPort,
                    MikrotikId = binding.MikrotikId,
                    IpCliente = binding.IpNet!.Ip,
                    MacCliente = binding.CargueDetail!.MacWlan
                });
            }

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SuspendMkSetupDTO>(ex);
        }
    }

    // Para devolver el acceso alcanza con el id que quedo guardado al suspender y el tipo
    public async Task<ActionResponse<ReactivateMkSetupDTO>> GetReactivateSetupAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ReactivateMkSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var datos = new ReactivateMkSetupDTO();

            var contract = await _context.ContractClients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contract == null)
            {
                datos.Blocked = _localizer[nameof(Resource.Generic_IdNotFound)];
                return Ok(datos);
            }

            if (contract.ContractState != ContractState.Suspended)
            {
                datos.Blocked = "El contrato ya no se encuentra suspendido.";
                return Ok(datos);
            }

            //La misma validacion de integridad que corre la web antes de devolver el servicio
            var integridad = await _contractActivationIntegrityService.ValidateAsync(
                contract.ContractClientId, contract.CorporationId);

            if (!integridad.WasSuccess)
            {
                datos.Blocked = integridad.Message;
                return Ok(datos);
            }

            datos.UsaHotSpot = await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId);

            if (!datos.UsaHotSpot)
            {
                return Ok(datos);
            }

            //El id y el servidor que quedaron guardados al suspender: asi no depende de lo
            //que tenga el IpBinding hoy
            var suspension = await _context.ContractSuspendeds
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId && x.DateReactivated == null)
                .OrderByDescending(x => x.DateSuspended)
                .Select(x => new { x.MkIndex, x.ServerId })
                .FirstOrDefaultAsync();

            var mkIndex = suspension?.MkIndex;
            var serverId = suspension?.ServerId;

            //Si no quedo el dato guardado (suspensiones viejas), se toma del IpBinding
            if (string.IsNullOrWhiteSpace(mkIndex) || serverId == null)
            {
                var bind = await _context.ContractBinds
                    .AsNoTracking()
                    .Where(x => x.ContractClientId == contractClientId)
                    .Select(x => new { x.MikrotikId, x.ServerId })
                    .FirstOrDefaultAsync();

                mkIndex ??= bind?.MikrotikId;
                serverId ??= bind?.ServerId;
            }

            if (string.IsNullOrWhiteSpace(mkIndex) || serverId == null)
            {
                datos.Blocked = _localizer["Suspend_BindIncomplete"];
                return Ok(datos);
            }

            var server = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == serverId);

            if (server?.IpNetwork?.Ip == null)
            {
                datos.Blocked = _localizer["Suspend_BindIncomplete"];
                return Ok(datos);
            }

            var bypassedType = await _context.HotSpotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

            if (bypassedType == null)
            {
                datos.Blocked = _localizer["Suspend_NoBypassedType"];
                return Ok(datos);
            }

            datos.ServerId = server.ServerId;
            datos.ServerName = server.ServerName;
            datos.ServerIp = server.IpNetwork!.Ip;
            datos.Usuario = server.Usuario;
            datos.Clave = server.Clave;
            datos.ApiPort = server.ApiPort;
            datos.MkIndex = mkIndex;
            datos.TipoBypassed = bypassedType.TypeName;

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReactivateMkSetupDTO>(ex);
        }
    }

    private static ActionResponse<T> Ok<T>(T resultado)
    {
        return new ActionResponse<T> { WasSuccess = true, Result = resultado };
    }

    private static ActionResponse<T> Fail<T>(string mensaje)
    {
        return new ActionResponse<T> { WasSuccess = false, Message = mensaje };
    }
}
