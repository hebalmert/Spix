using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class SupplierUnitOfWork : ISupplierUnitOfWork
{
    private readonly ISupplierServices _supplierServices;

    public SupplierUnitOfWork(ISupplierServices supplierServices)
    {
        _supplierServices = supplierServices;
    }

    public async Task<ActionResponse<IEnumerable<Supplier>>> ComboAsync(string email) => await _supplierServices.ComboAsync(email);

    public async Task<ActionResponse<IEnumerable<Supplier>>> GetAsync(PaginationDTO pagination, string email) => await _supplierServices.GetAsync(pagination, email);

    public async Task<ActionResponse<Supplier>> GetAsync(Guid id) => await _supplierServices.GetAsync(id);

    public async Task<ActionResponse<Supplier>> UpdateAsync(Supplier modelo, string frontUrl) => await _supplierServices.UpdateAsync(modelo, frontUrl);

    public async Task<ActionResponse<Supplier>> AddAsync(Supplier modelo, string email, string frontUrl) => await _supplierServices.AddAsync(modelo, email, frontUrl);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _supplierServices.DeleteAsync(id);
}