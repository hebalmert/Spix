using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class ProductStorageServiceX : IProductStorageServiceX
{
    private readonly IProductStorageService _productStorageService;

    public ProductStorageServiceX(IProductStorageService productStorageService)
    {
        _productStorageService = productStorageService;
    }

    public async Task<ActionResponse<IEnumerable<ProductStorage>>> ComboAsync(string username) => await _productStorageService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<StorageListItemDto>>> GetAsync(PaginationDTO pagination, string username) => await _productStorageService.GetAsync(pagination, username);

    public async Task<ActionResponse<ProductStorage>> GetAsync(Guid id, string username) => await _productStorageService.GetAsync(id, username);

    public async Task<ActionResponse<ProductStorage>> UpdateAsync(ProductStorage modelo, string username) => await _productStorageService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<ProductStorage>> AddAsync(ProductStorage modelo, string username) => await _productStorageService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _productStorageService.DeleteAsync(id, username);
}
