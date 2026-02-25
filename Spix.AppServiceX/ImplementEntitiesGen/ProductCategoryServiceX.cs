using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class ProductCategoryServiceX : IProductCategoryServiceX
{
    private readonly IProductCategoryService _productCategoryService;

    public ProductCategoryServiceX(IProductCategoryService productCategoryService)
    {
        _productCategoryService = productCategoryService;
    }

    public async Task<ActionResponse<IEnumerable<ProductCategory>>> ComboAsync(string username) => await _productCategoryService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<ProductCategory>>> GetAsync(PaginationDTO pagination, string username) => await _productCategoryService.GetAsync(pagination, username);

    public async Task<ActionResponse<ProductCategory>> GetAsync(Guid id) => await _productCategoryService.GetAsync(id);

    public async Task<ActionResponse<ProductCategory>> UpdateAsync(ProductCategory modelo) => await _productCategoryService.UpdateAsync(modelo);

    public async Task<ActionResponse<ProductCategory>> AddAsync(ProductCategory modelo, string username) => await _productCategoryService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _productCategoryService.DeleteAsync(id);
}