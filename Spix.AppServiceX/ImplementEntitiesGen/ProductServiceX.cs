using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class ProductServiceX : IProductServiceX
{
    private readonly IProductService _productService;

    public ProductServiceX(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ActionResponse<IEnumerable<Product>>> ComboAsync(string username, Guid id) => await _productService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<Product>>> GetAsync(PaginationDTO pagination, string username) => await _productService.GetAsync(pagination, username);

    public async Task<ActionResponse<Product>> GetAsync(Guid id) => await _productService.GetAsync(id);

    public async Task<ActionResponse<Product>> UpdateAsync(Product modelo) => await _productService.UpdateAsync(modelo);

    public async Task<ActionResponse<Product>> AddAsync(Product modelo, string username) => await _productService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _productService.DeleteAsync(id);
}