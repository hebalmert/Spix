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
                var activateBindingsResponse = await _contractActivationIntegrityService
                    .ActivateHotSpotBindingsAsync(contract);
                if (!activateBindingsResponse.WasSuccess)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return Fail<ContractSuspendedDTO>(activateBindingsResponse.Message!);
                }
            }

            contract.ContractState = ContractState.Active;

            var audit = new ContractSuspendedAudit
            {
                ContractId = contract.ContractClientId,
                ClientId = contract.ClientId,
                DateModified = DateTime.UtcNow,
                UserId = Guid.Parse(user.Id),
                UserByName = $"{user.FirstName} {user.LastName}",
                CorporationId = contract.CorporationId
            };

            _context.ContractSuspendedAudits.Add(audit);

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
}
