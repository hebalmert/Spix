using Spix.AppService.InterfacesDashboard;
using Spix.AppServiceX.InterfacesDashboard;
using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementDashboard;

public class DashboardServiceX : IDashboardServiceX
{
    private readonly IDashboardService _dashboardService;

    public DashboardServiceX(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<ActionResponse<DashboardSummaryDto>> GetSummaryAsync(string username) =>
        await _dashboardService.GetSummaryAsync(username);
}
