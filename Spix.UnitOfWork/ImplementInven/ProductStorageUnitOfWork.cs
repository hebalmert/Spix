using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class ProductStorageUnitOfWork : IProductStorageUnitOfWork
{
    private readonly IProductStorageService _productStorageService;

    public ProductStorageUnitOfWork(IProductStorageService productStorageService)
    {
        _productStorageService = productStorageService;
    }

    public async Task<ActionResponse<IEnumerable<ProductStorage>>> ComboAsync(string email) => await _productStorageService.ComboAsync(email);

    public async Task<ActionResponse<IEnumerable<ProductStorage>>> GetAsync(PaginationDTO pagination, string email) => await _productStorageService.GetAsync(pagination, email);

    public async Task<ActionResponse<ProductStorage>> GetAsync(Guid id) => await _productStorageService.GetAsync(id);

    public async Task<ActionResponse<ProductStorage>> UpdateAsync(ProductStorage modelo) => await _productStorageService.UpdateAsync(modelo);

    public async Task<ActionResponse<ProductStorage>> AddAsync(ProductStorage modelo, string email) => await _productStorageService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _productStorageService.DeleteAsync(id);
}