using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementInven;

public class ReportStockServiceX : IReportStockServiceX
{
    private readonly IReportStockService _reportStockService;

    public ReportStockServiceX(IReportStockService reportStockService)
    {
        _reportStockService = reportStockService;
    }

    public async Task<ActionResponse<ReportStockSummaryDto>> GetSummaryAsync(string username, DateTime? desde, DateTime? hasta, Guid? storageId)
        => await _reportStockService.GetSummaryAsync(username, desde, hasta, storageId);

    public async Task<ActionResponse<IEnumerable<ReportStockMoveDto>>> GetMovesAsync(string username, DateTime? desde, DateTime? hasta, Guid? storageId)
        => await _reportStockService.GetMovesAsync(username, desde, hasta, storageId);

    public async Task<ActionResponse<IEnumerable<ReportStockBalanceDto>>> GetBalanceAsync(string username, Guid? storageId)
        => await _reportStockService.GetBalanceAsync(username, storageId);
}
