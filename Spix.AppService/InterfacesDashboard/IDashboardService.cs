using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesDashboard;

public interface IDashboardService
{
    Task<ActionResponse<DashboardSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<SaasDashboardSummaryDto>> GetSaasSummaryAsync(string username);
}
