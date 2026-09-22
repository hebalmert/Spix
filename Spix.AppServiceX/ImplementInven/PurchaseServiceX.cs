using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ReportsDTO;

namespace Spix.AppServiceX.ImplementInven;

public class PurchaseServiceX : IPurchaseServiceX
{
    private readonly IPurchaseService _purchaseService;

    public PurchaseServiceX(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus() => await _purchaseService.GetComboStatus();

    public async Task<ActionResponse<IEnumerable<Purchase>>> GetReporteSellDates(ReportDataDTO pagination, string username) => await _purchaseService.GetReporteSellDates(pagination, username);

    public async Task<ActionResponse<PurchaseSummaryDto>> GetSummaryAsync(string username) => await _purchaseService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<Purchase>>> GetAsync(PaginationDTO pagination, string username) => await _purchaseService.GetAsync(pagination, username);

    public async Task<ActionResponse<Purchase>> GetAsync(Guid id, string username) => await _purchaseService.GetAsync(id, username);

    public async Task<ActionResponse<Purchase>> UpdateAsync(Purchase modelo, string username) => await _purchaseService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Purchase>> AddAsync(Purchase modelo, string username) => await _purchaseService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _purchaseService.DeleteAsync(id, username);
}
