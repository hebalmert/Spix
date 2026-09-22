using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ReportsDTO;

namespace Spix.AppServiceX.InterfacesInven;

public interface IPurchaseServiceX
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<Purchase>>> GetReporteSellDates(ReportDataDTO pagination, string username);

    Task<ActionResponse<PurchaseSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<Purchase>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Purchase>> GetAsync(Guid id, string username);

    Task<ActionResponse<Purchase>> UpdateAsync(Purchase modelo, string username);

    Task<ActionResponse<Purchase>> AddAsync(Purchase modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
