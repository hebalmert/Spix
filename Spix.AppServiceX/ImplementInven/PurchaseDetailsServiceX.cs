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

    public async Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string username) => await _purchaseDetailsService.GetAsync(pagination, username);

    public async Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id, string username) => await _purchaseDetailsService.GetAsync(id, username);

    public async Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo, string username) => await _purchaseDetailsService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string username) => await _purchaseDetailsService.AddAsync(modelo, username);

    public async Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string username) => await _purchaseDetailsService.ClosePurchaseSync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _purchaseDetailsService.DeleteAsync(id, username);
}
