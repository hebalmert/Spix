using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class ProductStockServiceX : IProductStockServiceX
{
    private readonly IProductStockService _productStockService;

    public ProductStockServiceX(IProductStockService productStockService)
    {
        _productStockService = productStockService;
    }

    public async Task<ActionResponse<IEnumerable<ProductStock>>> GetAsync(PaginationDTO pagination, string email) => await _productStockService.GetAsync(pagination, email);

    public async Task<ActionResponse<ProductStock>> GetAsync(Guid id) => await _productStockService.GetAsync(id);

    public async Task<ActionResponse<TransferStockDTO>> GetProductStock(TransferStockDTO modelo) => await _productStockService.GetProductStock(modelo);
}