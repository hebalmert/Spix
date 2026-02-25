using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesGen;

public interface IProductService
{
    Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Product>> GetAsync(Guid id);

    Task<ActionResponse<Product>> UpdateAsync(Product modelo);

    Task<ActionResponse<Product>> AddAsync(Product modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}