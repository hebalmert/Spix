using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementInven;

public class PurchaseDetailsServiceX : IPurchaseDetailsServiceX
{
    private readonly IPurchaseDetailsService _purchaseDetailsService;

    public PurchaseDetailsServiceX(IPurchaseDetailsService purchaseDetailsService)
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