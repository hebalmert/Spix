using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface IPurchaseDetailsServiceX
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id, string username);

    Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo, string username);

    Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string username);

    Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
