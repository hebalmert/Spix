using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementInven;

public class ReportInventoryServiceX : IReportInventoryServiceX
{
    private readonly IReportInventoryService _reportInventoryService;

    public ReportInventoryServiceX(IReportInventoryService reportInventoryService)
    {
        _reportInventoryService = reportInventoryService;
    }

    public async Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username)
    {
        return await _reportInventoryService.GetSerialSummaryAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username)
    {
        return await _reportInventoryService.GetSerialsAsync(username);
    }
}
