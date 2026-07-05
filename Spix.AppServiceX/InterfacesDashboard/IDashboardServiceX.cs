using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesDashboard;

public interface IDashboardServiceX
{
    Task<ActionResponse<DashboardSummaryDto>> GetSummaryAsync(string username);
}
