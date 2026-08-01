using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

public class RunSuspendedService : IRunSuspendedService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public RunSuspendedService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService,
        IContractActivationIntegrityService contractActivationIntegrityService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
        _contractActivationIntegrityService = contractActivationIntegrityService;
    }

    public async Task<ActionResponse<IEnumerable<RunSuspended>>> GetAsync(
        PaginationDTO pagination,
        string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<RunSuspended>>();
            }

            var queryable = _context.RunSuspendeds
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.YearNumber.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.MonthType.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.UserByName!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.YearNumber)
                .ThenByDescending(x => x.MonthType)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<RunSuspended>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<RunSuspended>>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> GetByIdAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            var model = await _context.RunSuspendeds
                .Include(x => x.RunSuspendedDetails!)
                    .ThenInclude(x => x.ContractClient)
                .Include(x => x.RunSuspendedDetails!)
                    .ThenInclude(x => x.Client)
                .Include(x => x.RunSuspendedDetails!)
                    .ThenInclude(x => x.CxCBill)
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id &&
                                          x.CorporationId == user.CorporationId);
            if (model == null)
            {
                return Fail<RunSuspended>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = model
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> AddAsync(RunSuspended model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            if (!IsValidPeriod(model.YearNumber, model.MonthType))
            {
                return Fail<RunSuspended>("Debe seleccionar un mes y a\u00f1o validos.");
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var exists = await PeriodExistsAsync(Guid.Empty, corporationId, model.YearNumber, model.MonthType);
            if (exists)
            {
                return Fail<RunSuspended>("Ya existe un corte general para ese mes y a\u00f1o.");
            }

            model.CorporationId = corporationId;
            model.Executed = false;
            model.DateUtc = null;
            model.UserId = null;
            model.UserByName = null;

            _context.RunSuspendeds.Add(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = model
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> UpdateAsync(RunSuspended model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            var current = await _context.RunSuspendeds
                .FirstOrDefaultAsync(x => x.RunSuspendedId == model.RunSuspendedId &&
                                          x.CorporationId == user.CorporationId);
            if (current == null)
            {
                return Fail<RunSuspended>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (current.Executed)
            {
                return Fail<RunSuspended>("El corte general ya fue ejecutado y no puede modificarse.");
            }

            if (!IsValidPeriod(model.YearNumber, model.MonthType))
            {
                return Fail<RunSuspended>("Debe seleccionar un mes y a\u00f1o validos.");
            }

            var exists = await PeriodExistsAsync(
                current.RunSuspendedId,
                current.CorporationId,
                model.YearNumber,
                model.MonthType);
            if (exists)
            {
                return Fail<RunSuspended>("Ya existe un corte general para ese mes y a\u00f1o.");
            }

            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;
            await _context.SaveChangesAsync();

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = current
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<bool>();
            }

            var model = await _context.RunSuspendeds
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id &&
                                          x.CorporationId == user.CorporationId);
            if (model == null)
            {
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (model.Executed)
            {
                return Fail<bool>("El corte general ya fue ejecutado y no puede eliminarse.");
            }

            _context.RunSuspendeds.Remove(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> RunAsync(Guid id, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var run = await _context.RunSuspendeds
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id &&
                                          x.CorporationId == corporationId);
            if (run == null)
            {
                return Fail<RunSuspended>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (run.Executed)
            {
                return Fail<RunSuspended>("El corte general ya fue ejecutado.");
            }

            var activeContracts = await _context.ContractClients
                .Include(x => x.Client)
                .Include(x => x.ContractPlans!)
                    .ThenInclude(x => x.Plan)
                .Where(x => x.CorporationId == corporationId &&
                            x.ContractState == ContractState.Active)
                .OrderBy(x => x.ControlContrato)
                .ToListAsync();

            if (activeContracts.Count == 0)
            {
                return Fail<RunSuspended>("No hay contratos activos para procesar en el corte general.");
            }

            var usesHotSpotControl = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(corporationId);

            if (usesHotSpotControl)
            {
                var validationResponse = await ValidateHotSpotConfigurationAsync(activeContracts);
                if (!validationResponse.WasSuccess)
                {
                    return Fail<RunSuspended>(validationResponse.Message!);
                }

                var regularTypeExists = await _context.HotSpotTypes
                    .AnyAsync(x => x.Active && x.TypeName == "regular");
                if (!regularTypeExists)
                {
                    return Fail<RunSuspended>("No existe un tipo de HotSpot activo con el valor regular.");
                }
            }

            var activeContractIds = activeContracts.Select(x => x.ContractClientId).ToList();
            var bills = await _context.CxCBills
                .Where(x => x.CorporationId == corporationId &&
                            activeContractIds.Contains(x.ContractClientId) &&
                            !x.Cancelled &&
                            x.Balance > 0)
                .OrderBy(x => x.DateNote)
                .ThenBy(x => x.CollectionNote)
                .ToListAsync();

            var debtByContract = bills
                .GroupBy(x => x.ContractClientId)
                .ToDictionary(x => x.Key, x => x.First());
            var contractsToSuspend = activeContracts
                .Where(x => debtByContract.ContainsKey(x.ContractClientId))
                .ToList();

            if (usesHotSpotControl)
            {
                var contractIdsToSuspend = contractsToSuspend
                    .Select(x => x.ContractClientId)
                    .ToList();
                var connectionResponse = await _contractActivationIntegrityService
                    .VerifyHotSpotServersConnectionAsync(contractIdsToSuspend);
                if (!connectionResponse.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<RunSuspended>(connectionResponse.Message!);
                }
            }

            var utcNow = DateTime.UtcNow;
            if (usesHotSpotControl)
            {
                var suspendResponse = await _contractActivationIntegrityService
                    .SuspendHotSpotBindingsAsync(contractsToSuspend);
                if (!suspendResponse.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<RunSuspended>(suspendResponse.Message!);
                }
            }

            foreach (var contract in contractsToSuspend)
            {
                var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
                var planAmount = contractPlan?.Plan?.Price ?? 0;
                var bill = debtByContract[contract.ContractClientId];

                contract.ContractState = ContractState.Suspended;
                _context.RunSuspendedDetails.Add(new RunSuspendedDetail
                {
                    RunSuspendedId = run.RunSuspendedId,
                    ContractClientId = contract.ContractClientId,
                    ClientId = contract.ClientId,
                    CxCBillId = bill.CxCBillId,
                    DateUtc = utcNow,
                    PlanAmount = planAmount
                });
            }

            run.Executed = true;
            run.DateUtc = utcNow;
            run.UserId = Guid.Parse(user.Id);
            run.UserByName = $"{user.FirstName} {user.LastName}";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = run
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<IntItemModel>>();
            }

            var list = _enumMultilLanguageService.GetEnumSelectList<MonthType>("Select_Month");
            return new ActionResponse<IEnumerable<IntItemModel>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    private async Task<ActionResponse<bool>> ValidateHotSpotConfigurationAsync(
        List<ContractClient> activeContracts)
    {
        var contractIds = activeContracts.Select(x => x.ContractClientId).ToList();
        var bindContractIds = await _context.ContractBinds
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();
        var queueContractIds = await _context.ContractQues
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();

        var bindIds = bindContractIds.ToHashSet();
        var queueIds = queueContractIds.ToHashSet();
        var contractsWithoutConfiguration = activeContracts
            .Where(x => !bindIds.Contains(x.ContractClientId) || !queueIds.Contains(x.ContractClientId))
            .Select(x => x.ControlContrato)
            .ToList();

        if (contractsWithoutConfiguration.Count == 0)
        {
            return Success<bool>();
        }

        var contractsText = string.Join(", ", contractsWithoutConfiguration.Take(10));
        var message = $"No se puede correr el corte general porque {contractsWithoutConfiguration.Count} contrato(s) activo(s) no tienen Contract Queue e IpBinding configurados. Contratos: {contractsText}.";
        return Fail<bool>(message);
    }

    private async Task<bool> PeriodExistsAsync(
        Guid runSuspendedId,
        int corporationId,
        int yearNumber,
        MonthType monthType)
    {
        return await _context.RunSuspendeds.AnyAsync(x =>
            x.RunSuspendedId != runSuspendedId &&
            x.CorporationId == corporationId &&
            x.YearNumber == yearNumber &&
            x.MonthType == monthType);
    }

    private static bool IsValidPeriod(int yearNumber, MonthType monthType)
    {
        return yearNumber >= 2000 && Enum.IsDefined(monthType);
    }

    private ActionResponse<T> AuthFail<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
        };
    }

    private static ActionResponse<T> Success<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = true,
            Result = default
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
