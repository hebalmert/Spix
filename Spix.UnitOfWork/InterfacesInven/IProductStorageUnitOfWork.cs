using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesInven;

public interface IProductStorageUnitOfWork
{
    Task<ActionResponse<IEnumerable<ProductStorage>>> ComboAsync(string email);

    Task<ActionResponse<IEnumerable<ProductStorage>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<ProductStorage>> GetAsync(Guid id);

    Task<ActionResponse<ProductStorage>> UpdateAsync(ProductStorage modelo);

    Task<ActionResponse<ProductStorage>> AddAsync(ProductStorage modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}