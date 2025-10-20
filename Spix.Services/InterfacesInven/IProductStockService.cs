using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesInven;

public interface IProductStockService
{
    Task<ActionResponse<IEnumerable<ProductStock>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<ProductStock>> GetAsync(Guid id);

    Task<ActionResponse<TransferStockDTO>> GetProductStock(TransferStockDTO modelo);
}