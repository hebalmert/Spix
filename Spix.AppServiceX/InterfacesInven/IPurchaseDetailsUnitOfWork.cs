using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface IPurchaseDetailsUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id);

    Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo);

    Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string email);

    Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}