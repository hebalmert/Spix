using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ReportsDTO;

namespace Spix.AppService.InterfacesInven;

public interface IPurchaseService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<Purchase>>> GetReporteSellDates(ReportDataDTO pagination, string email);

    Task<ActionResponse<IEnumerable<Purchase>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Purchase>> GetAsync(Guid id);

    Task<ActionResponse<Purchase>> UpdateAsync(Purchase modelo);

    Task<ActionResponse<Purchase>> AddAsync(Purchase modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}