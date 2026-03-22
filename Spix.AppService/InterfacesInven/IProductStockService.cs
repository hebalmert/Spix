using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesInven;

public interface IProductStockService
{
    Task<ActionResponse<IEnumerable<ProductStock>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<ProductStock>> GetAsync(Guid id);

    Task<ActionResponse<TransferStockDTO>> GetProductStock(TransferStockDTO modelo);
}