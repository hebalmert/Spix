using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class ProductCategoryUnitOfWork : IProductCategoryUnitOfWork
{
    private readonly IProductCategoryService _productCategoryService;

    public ProductCategoryUnitOfWork(IProductCategoryService productCategoryService)
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