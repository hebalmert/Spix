using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class SupplierServiceX : ISupplierServiceX
{
    private readonly ISupplierService _supplierService;

    public SupplierServiceX(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<ActionResponse<IEnumerable<Supplier>>> ComboAsync(string username) => await _supplierService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<SupplierListItemDto>>> GetAsync(PaginationDTO pagination, string username) => await _supplierService.GetAsync(pagination, username);

    public async Task<ActionResponse<Supplier>> GetAsync(Guid id, string username) => await _supplierService.GetAsync(id, username);

    public async Task<ActionResponse<Supplier>> UpdateAsync(Supplier modelo, string username) => await _supplierService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Supplier>> AddAsync(Supplier modelo, string username) => await _supplierService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _supplierService.DeleteAsync(id, username);
}
