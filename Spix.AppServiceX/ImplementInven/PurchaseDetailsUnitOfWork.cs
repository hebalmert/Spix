using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class PurchaseDetailsUnitOfWork : IPurchaseDetailsUnitOfWork
{
    private readonly IPurchaseDetailsService _purchaseDetailsService;

    public PurchaseDetailsUnitOfWork(IPurchaseDetailsService purchaseDetailsService)
    {
        _purchaseDetailsService = purchaseDetailsService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus() => await _purchaseDetailsService.GetComboStatus();

    public async Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string email) => await _purchaseDetailsService.GetAsync(pagination, email);

    public async Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id) => await _purchaseDetailsService.GetAsync(id);

    public async Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo) => await _purchaseDetailsService.UpdateAsync(modelo);

    public async Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string email) => await _purchaseDetailsService.AddAsync(modelo, email);

    public async Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string email) => await _purchaseDetailsService.ClosePurchaseSync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _purchaseDetailsService.DeleteAsync(id);
}