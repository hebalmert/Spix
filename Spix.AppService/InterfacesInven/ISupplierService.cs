using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesInven;

public interface ISupplierService
{
    Task<ActionResponse<IEnumerable<Supplier>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<SupplierListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Supplier>> GetAsync(Guid id, string username);

    Task<ActionResponse<Supplier>> UpdateAsync(Supplier modelo, string username);

    Task<ActionResponse<Supplier>> AddAsync(Supplier modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
