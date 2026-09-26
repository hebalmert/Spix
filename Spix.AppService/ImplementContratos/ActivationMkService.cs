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

// Reactivacion masiva DESDE EL ESCRITORIO, equipo por equipo.
//
// Por que existe: el Backend le habla al MikroTik por IP publica; el escritorio le habla
// por la red LAN, que es la unica forma de atender a un cliente sin IP publica. Entonces
// el trabajo va partido: aqui se junta el lote de un servidor con sus datos de conexion,
// el escritorio escribe el equipo, y despues vuelve aqui a guardar lo que el equipo acepto.
//
// Quien entra en el lote se decide de ESTE lado, con la misma consulta que usa la web. Este
// servicio nunca abre una conexion al equipo.
public partial class ActivationMkService : IActivationMkService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public ActivationMkService(
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

    // El lote de un equipo: con quien hablar, que tipo ponerles y a quienes
    public async Task<ActionResponse<ActivationMkSetupDTO>> GetActivateSetupAsync(Guid serverId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<ActivationMkSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var datos = new ActivationMkSetupDTO { ServerId = serverId };

            //Los contratos de ese equipo que esperan reactivacion: la MISMA consulta de la web
            var ids = await PendingQuery(corporationId)
                .Where(x => _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId &&
                                                            b.ServerId == serverId) &&
                            _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId))
                .Select(x => x.ContractClientId)
                .ToListAsync();

            if (ids.Count == 0)
            {
                return Ok(datos);
            }

            var contracts = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .Where(x => x.CorporationId == corporationId &&
                            x.ContractState == ContractState.Suspended &&
                            ids.Contains(x.ContractClientId))
                .ToListAsync();

            if (contracts.Count == 0)
            {
                return Ok(datos);
            }

            datos.UsaHotSpot = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(corporationId);

            var contractIds = contracts.Select(x => x.ContractClientId).ToList();

            var bindings = await _context.ContractBinds
                .AsNoTracking()
                .Include(x => x.Server!)
                    .ThenInclude(x => x.IpNetwork)
                .Include(x => x.IpNet)
                .Include(x => x.CargueDetail)
                .Where(x => x.ServerId == serverId && contractIds.Contains(x.ContractClientId))
                .ToListAsync();

            //Sin HotSpot no se toca el equipo: el lote viaja solo para que el escritorio
            //sepa a quienes darle por reactivados
            if (!datos.UsaHotSpot)
            {
                foreach (var contract in contracts)
                {
                    datos.Bindings.Add(NuevoBinding(contract, null));
                }

                return Ok(datos);
            }

            var server = bindings.FirstOrDefault()?.Server;

            if (server?.IpNetwork?.Ip == null)
            {
                datos.Blocked = _localizer[nameof(Resource.Mikrotik_Connection_Error)];
                return Ok(datos);
            }

            var bypassed = await _context.HotSpotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

            if (bypassed == null)
            {
                datos.Blocked = _localizer["Activation_NoBypassed"];
                return Ok(datos);
            }

            datos.ServerName = server.ServerName;
            datos.ServerIp = server.IpNetwork!.Ip;
            datos.Usuario = server.Usuario;
            datos.Clave = server.Clave;
            datos.ApiPort = server.ApiPort;
            datos.TipoBypassed = bypassed.TypeName;

            //Los que les falte algo viajan igual, con los campos vacios: el escritorio los
            //cuenta como que quedaron fuera, tal como hace la web, en vez de omitirlos
            foreach (var contract in contracts)
            {
                var binding = bindings.FirstOrDefault(x => x.ContractClientId == contract.ContractClientId);

                datos.Bindings.Add(NuevoBinding(contract, binding));
            }

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ActivationMkSetupDTO>(ex);
        }
    }

    // El comentario se arma AQUI, byte a byte igual que en el Backend, para que el registro
    // del equipo quede identico se reactive desde donde se reactive
    private static ActivationMkBindingDTO NuevoBinding(ContractClient contract, ContractBind? binding)
    {
        var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim();

        return new ActivationMkBindingDTO
        {
            ContractClientId = contract.ContractClientId,
            ControlContrato = contract.ControlContrato,
            ClientFullName = clientName,
            MikrotikId = binding?.MikrotikId,
            IpCliente = binding?.IpNet?.Ip,
            MacCliente = binding?.CargueDetail?.MacWlan,
            Comentario = $"{clientName} - ({contract.ControlContrato})"
        };
    }

    //Los que esperan reactivacion: cortados por el corte, con pago recibido y sin reactivar.
    //Es la misma puerta que usa la web: el resto de suspensiones se levantan a mano.
    private IQueryable<ContractSuspended> PendingQuery(int corporationId) =>
        _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.DateReactivated == null &&
                        x.PaymentReceived &&
                        x.Origin == SuspendedOrigin.Corte);

    private static ActionResponse<T> Ok<T>(T resultado)
    {
        return new ActionResponse<T> { WasSuccess = true, Result = resultado };
    }

    private static ActionResponse<T> Fail<T>(string mensaje)
    {
        return new ActionResponse<T> { WasSuccess = false, Message = mensaje };
    }
}
