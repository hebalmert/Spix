using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementContratos;

//La reactivacion de los que se cortaron por falta de pago y ya pagaron.
//
//Es el proceso inverso del corte, y por la misma razon va aparte: al recibir el pago NO se
//toca la Mikrotik, porque serian conexiones sueltas a toda hora desde la oficina y desde
//cada tecnico en la calle. Aqui se hace una sola vez, ordenado y equipo por equipo.
public class ActivationService : IActivationService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public ActivationService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IContractActivationIntegrityService contractActivationIntegrityService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _contractActivationIntegrityService = contractActivationIntegrityService;
    }

    //Revision previa: cuantos esperan reactivacion y como quedan repartidos por equipo.
    //Los numeros los cuenta la base: aqui no viaja ni un contrato.
    public async Task<ActionResponse<ActivationCheckDto>> CheckAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ActivationCheckDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Los que pagaron estando cortados, agrupados por el equipo donde viven
            var grupos = await PendingQuery(corporationId)
                .Select(x => new
                {
                    x.ContractClientId,
                    ServerId = _context.ContractBinds
                        .Where(b => b.ContractClientId == x.ContractClientId)
                        .Select(b => (Guid?)b.ServerId)
                        .FirstOrDefault(),
                    ServerName = _context.ContractBinds
                        .Where(b => b.ContractClientId == x.ContractClientId)
                        .Select(b => b.Server!.ServerName)
                        .FirstOrDefault(),
                    HasQueue = _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId)
                })
                .GroupBy(x => new { x.ServerId, x.ServerName, x.HasQueue })
                .Select(g => new
                {
                    g.Key.ServerId,
                    g.Key.ServerName,
                    g.Key.HasQueue,
                    Contracts = g.Count()
                })
                .ToListAsync();

            var dto = new ActivationCheckDto
            {
                Ready = grupos.Sum(x => x.Contracts),
                NoBinding = grupos.Where(x => x.ServerId == null).Sum(x => x.Contracts),
                NoQueue = grupos.Where(x => !x.HasQueue).Sum(x => x.Contracts)
            };

            //Solo se puede reactivar al que tiene IpBinding y Queue
            dto.Servers = grupos
                .Where(x => x.ServerId != null && x.HasQueue)
                .GroupBy(x => new { x.ServerId, x.ServerName })
                .Select(g => new ActivationServerDto
                {
                    ServerId = g.Key.ServerId!.Value,
                    ServerName = g.Key.ServerName,
                    Contracts = g.Sum(x => x.Contracts)
                })
                .OrderBy(x => x.ServerName)
                .ToList();

            dto.ToActivate = dto.Servers.Sum(x => x.Contracts);

            return new ActionResponse<ActivationCheckDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ActivationCheckDto>(ex);
        }
    }

    //Los que esperan reactivacion, pagina por pagina
    public async Task<ActionResponse<IEnumerable<ActivationDetailDto>>> GetPendingAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ActivationDetailDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var queryable = PendingQuery(corporationId)
                .Select(x => new ActivationDetailDto
                {
                    ControlContrato = x.ControlContrato,
                    ClientFullName = x.ClientName ?? string.Empty,
                    ZoneName = x.ZoneName,
                    ServerName = _context.ContractBinds
                        .Where(b => b.ContractClientId == x.ContractClientId)
                        .Select(b => b.Server!.ServerName)
                        .FirstOrDefault(),
                    DateSuspended = x.DateSuspended,
                    DatePaymentReceived = x.DatePaymentReceived,
                    HasBinding = _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId),
                    HasQueue = _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId)
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ActivationDetailDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ActivationDetailDto>>(ex);
        }
    }

    //Reactiva TODOS los contratos de UN servidor: se abre la conexion del equipo una sola
    //vez, se les devuelve el acceso a todos y se cierra. Si el equipo no responde, lo de
    //los demas ya quedo hecho y estos se pueden reintentar.
    public async Task<ActionResponse<ActivationRunResultDto>> RunServerAsync(Guid serverId, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await transaction.RollbackAsync();
                return AuthFail<ActivationRunResultDto>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);

            //Los contratos de ese equipo que esperan reactivacion
            var ids = await PendingQuery(corporationId)
                .Where(x => _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId &&
                                                            b.ServerId == serverId) &&
                            _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId))
                .Select(x => x.ContractClientId)
                .ToListAsync();

            var result = new ActivationRunResultDto { Contracts = ids.Count };

            if (ids.Count == 0)
            {
                await transaction.CommitAsync();
                return new ActionResponse<ActivationRunResultDto> { WasSuccess = true, Result = result };
            }

            //Sin Includes de coleccion: el cliente hace falta para el comentario del equipo
            var contracts = await _context.ContractClients
                .Include(x => x.Client)
                .Where(x => x.CorporationId == corporationId &&
                            x.ContractState == ContractState.Suspended &&
                            ids.Contains(x.ContractClientId))
                .ToListAsync();

            result.Skipped = ids.Count - contracts.Count;

            if (contracts.Count == 0)
            {
                await transaction.CommitAsync();
                return new ActionResponse<ActivationRunResultDto> { WasSuccess = true, Result = result };
            }

            //Si la corporacion controla el acceso por HotSpot, se le devuelve en el equipo.
            //Este modulo habla con la Mikrotik por su cuenta: asi el corte y las demas
            //pantallas siguen con su propio codigo, sin enterarse de lo que pase aqui.
            var usesHotSpotControl = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(corporationId);

            var activados = contracts;

            if (usesHotSpotControl)
            {
                var respuesta = await ActivateOnServerAsync(serverId, contracts, result);
                if (!respuesta.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<ActivationRunResultDto>(respuesta.Message!);
                }

                //Solo se dan por activados los que el equipo si acepto
                activados = respuesta.Result!;
            }

            var userName = $"{user.FirstName} {user.LastName}";
            var userId = Guid.Parse(user.Id);

            foreach (var contract in activados)
            {
                contract.ContractState = ContractState.Active;

                //Se cierra la suspension: la fila queda como historia, no se borra
                await ContractSuspendedRegistry.CloseAsync(_context, contract.ContractClientId, userName, userId);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Activated = activados.Count;
            return new ActionResponse<ActivationRunResultDto> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<ActivationRunResultDto>(ex);
        }
    }

    //Le devuelve el acceso a los contratos de UN equipo: se abre la conexion una sola vez,
    //se recorren todos y se cierra. Si un contrato no responde, se anota y se sigue con el
    //siguiente: el que no quede se ve en la pantalla y se resuelve por Contratos Suspendidos.
    private async Task<ActionResponse<List<ContractClient>>> ActivateOnServerAsync(
        Guid serverId,
        List<ContractClient> contracts,
        ActivationRunResultDto result)
    {
        var ids = contracts.Select(x => x.ContractClientId).ToList();

        var bindings = await _context.ContractBinds
            .Include(x => x.Server!)
                .ThenInclude(x => x.IpNetwork)
            .Include(x => x.IpNet)
            .Include(x => x.CargueDetail)
            .Where(x => x.ServerId == serverId && ids.Contains(x.ContractClientId))
            .ToListAsync();

        var server = bindings.FirstOrDefault()?.Server;
        if (server?.IpNetwork?.Ip == null)
            return Fail<List<ContractClient>>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);

        //El tipo que deja pasar el trafico: es lo contrario de lo que pone el corte
        var bypassed = await _context.HotSpotTypes
            .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

        if (bypassed == null)
            return Fail<List<ContractClient>>(_localizer["Activation_NoBypassed"]);

        var activados = new List<ContractClient>();
        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(server.IpNetwork.Ip, server.ApiPort);
            if (!mikrotik.Login(server.Usuario, server.Clave))
                return Fail<List<ContractClient>>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);

            foreach (var contract in contracts)
            {
                var binding = bindings.FirstOrDefault(x => x.ContractClientId == contract.ContractClientId);
                var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim();

                if (binding?.IpNet?.Ip == null ||
                    binding.CargueDetail?.MacWlan == null ||
                    string.IsNullOrWhiteSpace(binding.MikrotikId))
                {
                    result.Issues.Add(NewIssue(contract, _localizer["Activation_BindingIncomplete"]));
                    continue;
                }

                try
                {
                    mikrotik.Send("/ip/hotspot/ip-binding/set");
                    mikrotik.Send("=.id=" + binding.MikrotikId);
                    mikrotik.Send("=address=" + binding.IpNet.Ip);
                    mikrotik.Send("=to-address=" + binding.IpNet.Ip);
                    mikrotik.Send("=comment=" + $"{clientName} - ({contract.ControlContrato})");
                    mikrotik.Send("=mac-address=" + binding.CargueDetail.MacWlan);
                    mikrotik.Send("=server=all");
                    mikrotik.Send("=type=" + bypassed.TypeName, true);
                    mikrotik.Read();

                    binding.HotSpotTypeId = bypassed.HotSpotTypeId;
                    activados.Add(contract);
                }
                catch
                {
                    //Uno que no respondio no detiene a los demas
                    result.Issues.Add(NewIssue(contract, _localizer["Activation_DeviceFailed"]));
                }
            }
        }
        catch
        {
            return Fail<List<ContractClient>>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
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

        return new ActionResponse<List<ContractClient>> { WasSuccess = true, Result = activados };
    }

    private static ActivationIssueDto NewIssue(ContractClient contract, string reason) => new()
    {
        ControlContrato = contract.ControlContrato,
        ClientFullName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim(),
        Reason = reason
    };

    //Los que esperan reactivacion: cortados por el corte, con pago recibido y sin reactivar.
    //Es la unica puerta del modulo: el resto de suspensiones se levantan a mano.
    private IQueryable<ContractSuspended> PendingQuery(int corporationId) =>
        _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.DateReactivated == null &&
                        x.PaymentReceived &&
                        x.Origin == SuspendedOrigin.Corte);

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };

    private static ActionResponse<T> Fail<T>(string message) => new()
    {
        WasSuccess = false,
        Message = message
    };
}
