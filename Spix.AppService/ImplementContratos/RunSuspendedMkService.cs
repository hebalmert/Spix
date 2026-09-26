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

// El corte masivo DESDE EL ESCRITORIO, equipo por equipo.
//
// Por que existe: el Backend le habla al MikroTik por IP publica; el escritorio le habla
// por la red LAN, que es la unica forma de atender a un cliente sin IP publica. Entonces el
// trabajo va partido: aqui se arma el lote del equipo con sus datos de conexion, el
// escritorio le quita el acceso, y despues vuelve aqui a guardar lo que el equipo acepto.
//
// QUIEN entra en el lote lo decide ESTE lado, con la misma consulta de deuda que usa la web:
// cualquier nota no anulada con saldo, del mes que sea. Este servicio nunca abre una
// conexion al equipo.
public partial class RunSuspendedMkService : IRunSuspendedMkService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public RunSuspendedMkService(
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

    // El lote de un equipo: a quienes hay que cortarles y que escribirles
    public async Task<ActionResponse<CorteMkSetupDTO>> GetRunSetupAsync(Guid id, Guid serverId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return Fail<CorteMkSetupDTO>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var datos = new CorteMkSetupDTO { SinEquipo = serverId == Guid.Empty };

            var run = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id && x.CorporationId == corporationId);

            if (run == null)
            {
                datos.Blocked = _localizer[nameof(Resource.Generic_IdNotFound)];
                return Ok(datos);
            }

            if (run.Executed)
            {
                datos.Blocked = _localizer["Corte_AlreadyExecuted"];
                return Ok(datos);
            }

            var contracts = await ContratosDelLoteAsync(corporationId, serverId);

            if (contracts.Count == 0)
            {
                return Ok(datos);
            }

            datos.Contracts = contracts
                .Select(x => new CorteMkContractDTO
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientFullName = $"{x.Client?.FirstName} {x.Client?.LastName}".Trim()
                })
                .ToList();

            datos.UsaHotSpot = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(corporationId);

            //Sin HotSpot no se toca el equipo, y sin IpBinding no hay como quitarles el
            //acceso: en los dos casos no hay nada que escribir
            if (!datos.UsaHotSpot || datos.SinEquipo)
            {
                return Ok(datos);
            }

            var regularType = await _context.HotSpotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");

            if (regularType == null)
            {
                datos.Blocked = _localizer["Suspend_NoRegularType"];
                return Ok(datos);
            }

            datos.TipoRegular = regularType.TypeName;

            var ids = contracts.Select(x => x.ContractClientId).ToList();

            //TODOS los bindings de esos contratos, no solo los del equipo de la ruta: es lo
            //que hace la web, y un contrato podria tener binding en mas de un equipo
            var bindings = await _context.ContractBinds
                .AsNoTracking()
                .Include(x => x.Server!)
                    .ThenInclude(x => x.IpNetwork)
                .Include(x => x.IpNet)
                .Include(x => x.CargueDetail)
                .Where(x => ids.Contains(x.ContractClientId))
                .ToListAsync();

            foreach (var binding in bindings)
            {
                var contract = contracts.First(x => x.ContractClientId == binding.ContractClientId);
                var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim();

                //Si a uno le falta algo no se manda NI UNA orden del lote: es la regla de la
                //web, y a medio camino quedaria gente cortada y gente no
                if (binding.Server?.IpNetwork?.Ip == null ||
                    binding.IpNet?.Ip == null ||
                    binding.CargueDetail?.MacWlan == null ||
                    string.IsNullOrWhiteSpace(binding.MikrotikId))
                {
                    datos.Blocked = _localizer["Suspend_BindIncomplete"];
                    return Ok(datos);
                }

                datos.Bindings.Add(new CorteMkBindingDTO
                {
                    ContractClientId = binding.ContractClientId,
                    ControlContrato = contract.ControlContrato,
                    ClientFullName = clientName,
                    ServerId = binding.ServerId,
                    ServerName = binding.Server!.ServerName,
                    ServerIp = binding.Server.IpNetwork!.Ip,
                    Usuario = binding.Server.Usuario,
                    Clave = binding.Server.Clave,
                    ApiPort = binding.Server.ApiPort,
                    MikrotikId = binding.MikrotikId,
                    IpCliente = binding.IpNet!.Ip,
                    MacCliente = binding.CargueDetail!.MacWlan,
                    Comentario = $"{clientName} - ({contract.ControlContrato})"
                });
            }

            return Ok(datos);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CorteMkSetupDTO>(ex);
        }
    }

    // Los contratos de ese equipo que deben y todavia no estan suspendidos.
    // Es la misma cadena de filtros de la web, en el mismo orden.
    private async Task<List<ContractClient>> ContratosDelLoteAsync(int corporationId, Guid serverId)
    {
        //El equipo vacio es el de los que no tienen IpBinding: no viven en ningun equipo
        var delServidor = serverId == Guid.Empty
            ? await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && !x.ContractBinds!.Any())
                .Select(x => x.ContractClientId)
                .ToListAsync()
            : await _context.ContractBinds
                .AsNoTracking()
                .Where(x => x.ServerId == serverId && x.ContractClient!.CorporationId == corporationId)
                .Select(x => x.ContractClientId)
                .Distinct()
                .ToListAsync();

        if (delServidor.Count == 0)
        {
            return new List<ContractClient>();
        }

        //La deuda se vuelve a mirar: entre la revision y el corte alguien pudo pagar
        var deudores = await DeudoresAsync(corporationId, delServidor);
        var idsDelLote = delServidor.Where(deudores.Contains).ToList();

        if (idsDelLote.Count == 0)
        {
            return new List<ContractClient>();
        }

        //Los que ya estan suspendidos se quedan como estan
        var suspendidos = await SuspendidosAsync(corporationId);
        idsDelLote = idsDelLote.Where(x => !suspendidos.Contains(x)).ToList();

        if (idsDelLote.Count == 0)
        {
            return new List<ContractClient>();
        }

        return await _context.ContractClients
            .AsNoTracking()
            .Include(x => x.Client)
            .Where(x => x.CorporationId == corporationId && idsDelLote.Contains(x.ContractClientId))
            .ToListAsync();
    }

    // Quien debe: CUALQUIER nota no anulada con saldo, del mes que sea. El corte no mira
    // periodos a proposito, para que nadie quede navegando debiendo.
    private async Task<HashSet<Guid>> DeudoresAsync(int corporationId, List<Guid> soloEstos)
    {
        var ids = await _context.CxCBills
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        !x.Cancelled &&
                        x.Balance > 0 &&
                        soloEstos.Contains(x.ContractClientId))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }

    // Se miran los dos lados, el registro de suspension abierto y el estado del contrato,
    // que dicen lo mismo
    private async Task<HashSet<Guid>> SuspendidosAsync(int corporationId)
    {
        var conRegistro = await _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && x.DateReactivated == null)
            .Select(x => x.ContractClientId)
            .ToListAsync();

        var porEstado = await _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && x.ContractState == ContractState.Suspended)
            .Select(x => x.ContractClientId)
            .ToListAsync();

        var todos = conRegistro.ToHashSet();
        todos.UnionWith(porEstado);

        return todos;
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
