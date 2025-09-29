using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesGen;

public interface IProductCategoryService
{
    Task<ActionResponse<IEnumerable<ProductCategory>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<ProductCategory>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ProductCategory>> GetAsync(Guid id);

    Task<ActionResponse<ProductCategory>> UpdateAsync(ProductCategory modelo);

    Task<ActionResponse<ProductCategory>> AddAsync(ProductCategory modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}