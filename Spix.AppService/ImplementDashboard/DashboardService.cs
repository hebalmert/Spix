using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesDashboard;
using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementDashboard;

public class DashboardService : IDashboardService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public DashboardService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<DashboardSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user?.CorporationId == null)
                return AuthFail<DashboardSummaryDto>();

            var corporationId = user.CorporationId.Value;
            var today = DateTime.UtcNow;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var activeContracts = await _context.ContractClients
                .CountAsync(x => x.CorporationId == corporationId && x.ContractState == ContractState.Active);

            var suspendedContracts = await _context.ContractClients
                .CountAsync(x => x.CorporationId == corporationId && x.ContractState == ContractState.Suspended);

            var monthBills = _context.CxCBills
                .Where(x => x.CorporationId == corporationId &&
                            !x.Cancelled &&
                            x.DateNote >= monthStart &&
                            x.DateNote < nextMonth);

            var monthTotal = await monthBills.SumAsync(x => (decimal?)x.Total) ?? 0;
            var monthBalance = await monthBills.SumAsync(x => (decimal?)x.Balance) ?? 0;

            var monthCollected = await _context.CxCBillDetails
                .Where(x => x.CorporationId == corporationId &&
                            x.CxCBill != null &&
                            !x.CxCBill.Cancelled &&
                            x.CxCBill.DateNote >= monthStart &&
                            x.CxCBill.DateNote < nextMonth)
                .SumAsync(x => (decimal?)x.Payment) ?? 0;

            var summary = new DashboardSummaryDto
            {
                ActiveContracts = activeContracts,
                SuspendedContracts = suspendedContracts,
                MonthTotal = monthTotal,
                MonthCollected = monthCollected,
                MonthBalance = monthBalance
            };

            return new ActionResponse<DashboardSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<DashboardSummaryDto>(ex);
        }
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
