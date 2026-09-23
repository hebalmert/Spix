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

public class ContractExemptService : IContractExemptService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public ContractExemptService(
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

    //Listado del modulo, leido de la tabla de exoneraciones (la foto del momento).
    //desde/hasta filtran por fecha de exoneracion; soloAbiertas deja los que siguen exonerados.
    public async Task<ActionResponse<ExemptListDTO>> GetRecordsAsync(string? filter, DateTime? desde,
        DateTime? hasta, bool soloAbiertas, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<ExemptListDTO>();
            }

            var queryable = _context.ContractExempts
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
                queryable = queryable.Where(x => x.DateExempt >= desde.Value.Date);
            }

            if (hasta.HasValue)
            {
                var tope = hasta.Value.Date.AddDays(1);
                queryable = queryable.Where(x => x.DateExempt < tope);
            }

            if (soloAbiertas)
            {
                queryable = queryable.Where(x => x.DateEnded == null);
            }

            //Los totales salen de TODO el filtro, no de la pagina que se ve
            var resumen = await queryable
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    TotalAmount = g.Sum(x => x.PlanAmount),
                    OpenCount = g.Count(x => x.DateEnded == null),
                    OpenAmount = g.Where(x => x.DateEnded == null).Sum(x => x.PlanAmount)
                })
                .FirstOrDefaultAsync();

            var records = await queryable
                .OrderByDescending(x => x.DateExempt)
                .Take(300)
                .Select(x => new ExemptRecordDTO
                {
                    ContractExemptId = x.ContractExemptId,
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
                    DateExempt = x.DateExempt,
                    DateEnded = x.DateEnded,
                    Motivo = x.Motivo,
                    UserByName = x.UserByName,
                    UserByNameEnded = x.UserByNameEnded
                })
                .ToListAsync();

            return new ActionResponse<ExemptListDTO>
            {
                WasSuccess = true,
                Result = new ExemptListDTO
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
            return await _httpErrorHandler.HandleErrorAsync<ExemptListDTO>(ex);
        }
    }

    //Contratos ACTIVOS que se pueden exonerar. Alimenta el autocompletar del Create.
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

    //Exonerar un contrato. Reglas PROPIAS de este modulo, no las de Control de Contratos:
    //  1. El contrato tiene que estar Activo.
    //  2. Tiene que tener IpBinding y estar en bypassed, que es como luce uno con servicio.
    //  3. Se cambia el contrato a Exonerado.
    //  4. Y de ultimo, si todo salio bien, se crea el registro de la exoneracion.
    //Aqui NO se toca el MikroTik: el cliente sigue navegando igual, lo unico que cambia
    //es que no se le cobra. Todo dentro de la misma transaccion.
    public async Task<ActionResponse<bool>> ExemptAsync(Guid contractClientId, string? motivo, string username)
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

            //1. Solo se exonera lo que esta activo
            if (contract.ContractState != ContractState.Active)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Exempt_OnlyActive"]);
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
                    return Fail<bool>(_localizer["Exempt_NeedsBypassed"]);
                }
            }

            //3. El contrato pasa a Exonerado
            contract.ContractState = ContractState.Exempt;

            //4. Y queda el registro de la exoneracion
            var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
            var userName = $"{user.FirstName} {user.LastName}".Trim();

            await ContractExemptRegistry.OpenAsync(_context, contract, motivo, userName, userId);

            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Exempted,
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

    //Retirar la exoneracion: el contrato vuelve a Activo y el registro se cierra.
    //Tampoco se toca el MikroTik, el cliente nunca perdio el servicio.
    public async Task<ActionResponse<bool>> ActivateAsync(Guid contractClientId, string username)
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

            if (contract.ContractState != ContractState.Exempt)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Exempt_NotExempt"]);
            }

            contract.ContractState = ContractState.Active;

            var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
            var userName = $"{user.FirstName} {user.LastName}".Trim();

            //Se cierra la exoneracion abierta: la fila queda como historia, no se borra
            await ContractExemptRegistry.CloseAsync(_context, contract.ContractClientId, userName, userId);

            await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.ExemptRemoved,
                null, userName, userId, clientId: contract.ClientId, corporationId: contract.CorporationId);

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

    private IQueryable<ContractClient> ContractQuery()
    {
        return _context.ContractClients
            .Include(x => x.Client)
            .Include(x => x.Zone)
                .ThenInclude(x => x!.City)
            .Include(x => x.ContractPlans)!
                .ThenInclude(x => x.Plan);
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
}
