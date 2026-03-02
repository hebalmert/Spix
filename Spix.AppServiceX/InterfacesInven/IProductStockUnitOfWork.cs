using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface IProductStockUnitOfWork
{
    Task<ActionResponse<IEnumerable<ProductStock>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<ProductStock>> GetAsync(Guid id);

    Task<ActionResponse<TransferStockDTO>> GetProductStock(TransferStockDTO modelo);
}