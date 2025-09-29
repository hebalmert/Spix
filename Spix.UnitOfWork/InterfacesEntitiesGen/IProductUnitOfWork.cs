using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IProductUnitOfWork
{
    Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Product>> GetAsync(Guid id);

    Task<ActionResponse<Product>> UpdateAsync(Product modelo);

    Task<ActionResponse<Product>> AddAsync(Product modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}