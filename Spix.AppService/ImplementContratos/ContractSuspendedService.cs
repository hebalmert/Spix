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
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementContratos;

public class ContractSuspendedService : IContractSuspendedService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public ContractSuspendedService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IContractActivationIntegrityService contractActivationIntegrityService)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _contractActivationIntegrityService = contractActivationIntegrityService;
    }

    //Listado del modulo, leido de la tabla de suspensiones (la foto del momento).
    //desde/hasta filtran por fecha de suspension; soloAbiertas deja los que siguen suspendidos.
    public async Task<ActionResponse<SuspendedListDTO>> GetRecordsAsync(string? filter, DateTime? desde,
        DateTime? hasta, bool soloAbiertas, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<SuspendedListDTO>();
            }

            var queryable = _context.ContractSuspendeds
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var texto = filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ClientName!, $"%{texto}%") ||
                    EF.Functions.Like(x.ClientDocument!, $"%{texto}%"));
            }

            if (desde.HasValue)
            {
                queryable = queryable.Where(x => x.DateSuspended >= desde.Value.Date);
            }

            if (hasta.HasValue)
            {
                var tope = hasta.Value.Date.AddDays(1);
                queryable = queryable.Where(x => x.DateSuspended < tope);
            }

            if (soloAbiertas)
            {
                queryable = queryable.Where(x => x.DateReactivated == null);
            }

            //Los totales salen de TODO el filtro, no de la pagina que se ve
            var resumen = await queryable
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    TotalAmount = g.Sum(x => x.PlanAmount),
                    OpenCount = g.Count(x => x.DateReactivated == null),
                    OpenAmount = g.Where(x => x.DateReactivated == null).Sum(x => x.PlanAmount)
                })
                .FirstOrDefaultAsync();

            var records = await queryable
                .OrderByDescending(x => x.DateSuspended)
                .Take(300)
                .Select(x => new SuspendedRecordDTO
                {
                    ContractSuspendedId = x.ContractSuspendedId,
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientName = x.ClientName,
                    ClientDocument = x.ClientDocument,
                    ContractPhone = x.ContractPhone,
                    ContractAddress = x.ContractAddress,
                    CityName = x.CityName,
                    ZoneName = x.ZoneName,
                    PlanName = x.PlanName,
                    PlanAmount = x.PlanAmount,
                    DateSuspended = x.DateSuspended,
                    DateReactivated = x.DateReactivated,
                    Origin = x.Origin,
                    Motivo = x.Motivo,
                    UserByName = x.UserByName,
                    UserByNameReactivated = x.UserByNameReactivated
                })
                .ToListAsync();

            return new ActionResponse<SuspendedListDTO>
            {
                WasSuccess = true,
                Result = new SuspendedListDTO
                {
                    Records = records,
                    TotalCount = resumen?.TotalCount ?? 0,
                    TotalAmount = resumen?.TotalAmount ?? 0,
                    OpenCount = resumen?.OpenCount ?? 0,
                    OpenAmount = resumen?.OpenAmount ?? 0
                }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SuspendedListDTO>(ex);
        }
    }

    //Contratos ACTIVOS que se pueden suspender. Alimenta el autocompletar del Create.
    public async Task<ActionResponse<IEnumerable<ActiveContractDTO>>> SearchActiveAsync(string filter, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ActiveContractDTO>>();
            }

            filter = filter?.Trim() ?? string.Empty;
            if (filter.Length < 3)
            {
                return new ActionResponse<IEnumerable<ActiveContractDTO>>
                {
                    WasSuccess = true,
                    Result = Enumerable.Empty<ActiveContractDTO>()
                };
            }

            var contracts = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan)
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.ContractState == ContractState.Active &&
                            (EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.FirstName + " " + x.Client.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.Document, $"%{filter}%")))
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .Take(30)
                .AsSplitQuery()
                .ToListAsync();

            var result = contracts.Select(x => new ActiveContractDTO
            {
                ContractClientId = x.ContractClientId,
                ControlContrato = x.ControlContrato,
                ClientName = $"{x.Client?.FirstName} {x.Client?.LastName}".Trim(),
                ClientDocument = x.Client?.Document,
                Address = x.Address,
                PlanName = x.ContractPlans?.FirstOrDefault()?.Plan?.PlanName,
                PlanAmount = x.ContractPlans?.FirstOrDefault()?.Plan?.Price ?? 0
            }).ToList();

            return new ActionResponse<IEnumerable<ActiveContractDTO>> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ActiveContractDTO>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ContractSuspendedDTO>>();
            }

            filter = filter?.Trim() ?? string.Empty;
            if (filter.Length < 2)
            {
                return new ActionResponse<IEnumerable<ContractSuspendedDTO>>
                {
                    WasSuccess = true,
                    Result = Enumerable.Empty<ContractSuspendedDTO>()
                };
            }

            var contracts = await ContractQuery()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.ContractState == ContractState.Suspended &&
                            (EF.Functions.Like(x.Client!.Document, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.FirstName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.FirstName + " " + x.Client.LastName, $"%{filter}%")))
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .ThenBy(x => x.ControlContrato)
                .Take(30)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ContractSuspendedDTO>>
            {
                WasSuccess = true,
                Result = contracts.Select(ToDto).ToList()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractSuspendedDTO>>(ex);
        }
    }

    //Suspender un contrato. Reglas PROPIAS de este modulo, no las de Control de Contratos:
    //  1. El contrato tiene que estar Activo.
    //  2. Tiene que tener IpBinding y estar en bypassed, que es como luce uno con servicio.
    //  3. Se entra al MikroTik y se pasa a regular.
    //  4. Se cambia el contrato a Suspendido.
    //  5. Y de ultimo, si todo salio bien, se crea el registro de la suspension.
    //Todo dentro de la misma transaccion: si algo falla, no queda nada a medias.
    public async Task<ActionResponse<bool>> SuspendAsync(Guid contractClientId, string? motivo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            var contract = await ContractQuery()
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //1. Solo se suspende lo que esta activo
            if (contract.ContractState != ContractState.Active)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Suspend_OnlyActive"]);
            }

            bool usaHotSpot = await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId);

            if (usaHotSpot)
            {
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
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<bool>(_localizer["Suspend_NeedsBypassed"]);
                }

                //3. Se bloquea el acceso en el equipo (deja el binding en regular)
                var suspension = await SuspendOnMikrotikAsync(contract);
                if (!suspension.WasSuccess)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<bool>(suspension.Message!);
                }
            }

            //4. El contrato pasa a Suspendido
            contract.ContractState = ContractState.Suspended;

            //5. Y queda el registro de la suspension
            var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
            var userName = $"{user.FirstName} {user.LastName}".Trim();

            await ContractSuspendedRegistry.OpenAsync(_context, contract, SuspendedOrigin.Manual,
                motivo, null, userName, userId);

            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Suspended,
                motivo, userName, userId, clientId: contract.ClientId, corporationId: contract.CorporationId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractSuspendedDTO>();
            }

            var contract = await ContractQuery()
                .FirstOrDefaultAsync(x => x.ContractClientId == id &&
                                          x.CorporationId == user.CorporationId);
            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractSuspendedDTO>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (contract.ContractState != ContractState.Suspended)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractSuspendedDTO>("El contrato ya no se encuentra suspendido.");
            }

            var integrityResponse = await _contractActivationIntegrityService.ValidateAsync(
                contract.ContractClientId,
                contract.CorporationId);
            if (!integrityResponse.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractSuspendedDTO>(integrityResponse.Message!);
            }

            bool usesHotSpotControl = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(contract.CorporationId);

            if (usesHotSpotControl)
            {
                var activateBindingsResponse = await ReactivateOnMikrotikAsync(contract);
                if (!activateBindingsResponse.WasSuccess)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<ContractSuspendedDTO>(activateBindingsResponse.Message!);
                }
            }

            contract.ContractState = ContractState.Active;

            var auditUserId = Guid.Parse(user.Id);
            var auditUserName = $"{user.FirstName} {user.LastName}".Trim();

            //Se cierra la suspension abierta: la fila queda como historia, no se borra
            await ContractSuspendedRegistry.CloseAsync(_context, contract.ContractClientId,
                auditUserName, auditUserId);

            //La reactivacion vive en la bitacora del contrato, no en una tabla aparte
            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Reactivated,
                null, auditUserName, auditUserId, clientId: contract.ClientId,
                corporationId: contract.CorporationId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractSuspendedDTO>
            {
                WasSuccess = true,
                Result = ToDto(contract)
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractSuspendedDTO>(ex);
        }
    }

    private IQueryable<ContractClient> ContractQuery()
    {
        return _context.ContractClients
            .Include(x => x.Client)
            .Include(x => x.Zone)
                .ThenInclude(x => x!.City)
            .Include(x => x.ContractPlans)!
                .ThenInclude(x => x.Plan);
    }

    private static ContractSuspendedDTO ToDto(ContractClient contract)
    {
        var plan = contract.ContractPlans?.FirstOrDefault()?.Plan;

        return new ContractSuspendedDTO
        {
            ContractClientId = contract.ContractClientId,
            ControlContrato = contract.ControlContrato,
            ClientDocument = contract.Client?.Document ?? string.Empty,
            ClientFullName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim(),
            PhoneNumber = contract.PhoneNumber,
            Address = contract.Address,
            CityName = contract.Zone?.City?.Name,
            ZoneName = contract.Zone?.ZoneName,
            PlanName = plan?.PlanName
        };
    }

    private ActionResponse<T> AuthFail<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
        };
    }

    private static ActionResponse<T> Fail<T>(string message)
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = message
        };
    }

    //===================== MikroTik de este modulo =====================
    //El acceso al equipo vive aqui, igual que ContractBindService y ContractQueService
    //tienen el suyo. Asi, lo que se cambie para el corte masivo o para la activacion
    //no toca la suspension, ni al reves. La clase MK es solo el transporte.
    private async Task<ActionResponse<bool>> SuspendOnMikrotikAsync(ContractClient contract)
    {
        var bindings = await _context.ContractBinds
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Include(x => x.IpNet)
            .Include(x => x.CargueDetail)
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .ToListAsync();

        if (bindings.Count == 0)
        {
            return Fail<bool>(_localizer["Suspend_NeedsBypassed"]);
        }

        //Al suspender, el acceso queda en regular: el cliente cae en el portal del HotSpot
        var regularType = await _context.HotSpotTypes
            .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");

        if (regularType == null)
        {
            return Fail<bool>(_localizer["Suspend_NoRegularType"]);
        }

        foreach (var binding in bindings)
        {
            if (binding.Server?.IpNetwork?.Ip == null ||
                binding.IpNet?.Ip == null ||
                binding.CargueDetail?.MacWlan == null ||
                string.IsNullOrWhiteSpace(binding.MikrotikId))
            {
                return Fail<bool>(_localizer["Suspend_BindIncomplete"]);
            }
        }

        //Una conexion por servidor, no una por binding
        foreach (var serverBindings in bindings.GroupBy(x => x.ServerId))
        {
            var server = serverBindings.First().Server!;
            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return Fail<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                foreach (var binding in serverBindings)
                {
                    var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName} - ({contract.ControlContrato})";

                    mikrotik.Send("/ip/hotspot/ip-binding/set");
                    mikrotik.Send("=.id=" + binding.MikrotikId);
                    mikrotik.Send("=address=" + binding.IpNet!.Ip);
                    mikrotik.Send("=to-address=" + binding.IpNet.Ip);
                    mikrotik.Send("=comment=" + clientName);
                    mikrotik.Send("=mac-address=" + binding.CargueDetail!.MacWlan);
                    mikrotik.Send("=server=all");
                    mikrotik.Send("=type=" + regularType.TypeName, true);
                    mikrotik.Read();

                    //El tipo tambien queda actualizado en la base
                    binding.HotSpotTypeId = regularType.HotSpotTypeId;
                }
            }
            catch
            {
                return Fail<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
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

        return new ActionResponse<bool> { WasSuccess = true, Result = true };
    }

    //Devuelve el acceso en el equipo: el binding vuelve a bypassed.
    //Usa el MkIndex y el ServerId que quedaron guardados al suspender, asi el modulo
    //no depende de lo que tenga el IpBinding hoy.
    private async Task<ActionResponse<bool>> ReactivateOnMikrotikAsync(ContractClient contract)
    {
        var suspension = await _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.ContractClientId == contract.ContractClientId && x.DateReactivated == null)
            .OrderByDescending(x => x.DateSuspended)
            .Select(x => new { x.MkIndex, x.ServerId })
            .FirstOrDefaultAsync();

        //Si no quedo el dato guardado (suspensiones viejas), se toma del IpBinding
        var mkIndex = suspension?.MkIndex;
        var serverId = suspension?.ServerId;

        if (string.IsNullOrWhiteSpace(mkIndex) || serverId == null)
        {
            var bind = await _context.ContractBinds
                .AsNoTracking()
                .Where(x => x.ContractClientId == contract.ContractClientId)
                .Select(x => new { x.MikrotikId, x.ServerId })
                .FirstOrDefaultAsync();

            mkIndex ??= bind?.MikrotikId;
            serverId ??= bind?.ServerId;
        }

        if (string.IsNullOrWhiteSpace(mkIndex) || serverId == null)
        {
            return Fail<bool>(_localizer["Suspend_BindIncomplete"]);
        }

        var server = await _context.Servers
            .AsNoTracking()
            .Include(x => x.IpNetwork)
            .FirstOrDefaultAsync(x => x.ServerId == serverId);

        if (server?.IpNetwork?.Ip == null)
        {
            return Fail<bool>(_localizer["Suspend_BindIncomplete"]);
        }

        //Al reactivar, el acceso vuelve a bypassed: el cliente pasa sin portal
        var bypassedType = await _context.HotSpotTypes
            .FirstOrDefaultAsync(x => x.Active && x.TypeName == "bypassed");

        if (bypassedType == null)
        {
            return Fail<bool>(_localizer["Suspend_NoBypassedType"]);
        }

        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(server.IpNetwork.Ip!, server.ApiPort);

            if (!mikrotik.Login(server.Usuario, server.Clave))
            {
                return Fail<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }

            //En un set solo hace falta el id y lo que cambia
            mikrotik.Send("/ip/hotspot/ip-binding/set");
            mikrotik.Send("=.id=" + mkIndex);
            mikrotik.Send("=type=" + bypassedType.TypeName, true);
            mikrotik.Read();
        }
        catch
        {
            return Fail<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
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

        //El tipo tambien queda actualizado en la base
        var bindings = await _context.ContractBinds
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .ToListAsync();

        foreach (var binding in bindings)
        {
            binding.HotSpotTypeId = bypassedType.HotSpotTypeId;
        }

        return new ActionResponse<bool> { WasSuccess = true, Result = true };
    }
}
