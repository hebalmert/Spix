using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesInven;

public interface IProductStorageService
{
    Task<ActionResponse<IEnumerable<ProductStorage>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<StorageListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ProductStorage>> GetAsync(Guid id, string username);

    Task<ActionResponse<ProductStorage>> UpdateAsync(ProductStorage modelo, string username);

    Task<ActionResponse<ProductStorage>> AddAsync(ProductStorage modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
