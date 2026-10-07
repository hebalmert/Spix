using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesInven;

public interface IReportStockService
{
    Task<ActionResponse<ReportStockSummaryDto>> GetSummaryAsync(string username, DateTime? desde, DateTime? hasta, Guid? storageId);

    Task<ActionResponse<IEnumerable<ReportStockMoveDto>>> GetMovesAsync(string username, DateTime? desde, DateTime? hasta, Guid? storageId);

    Task<ActionResponse<IEnumerable<ReportStockBalanceDto>>> GetBalanceAsync(string username, Guid? storageId);
}
