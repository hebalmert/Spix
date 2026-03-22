using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
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

    public async Task<ActionResponse<IEnumerable<Purchase>>> GetReporteSellDates(ReportDataDTO pagination, string email) => await _purchaseService.GetReporteSellDates(pagination, email);

    public async Task<ActionResponse<IEnumerable<Purchase>>> GetAsync(PaginationDTO pagination, string email) => await _purchaseService.GetAsync(pagination, email);

    public async Task<ActionResponse<Purchase>> GetAsync(Guid id) => await _purchaseService.GetAsync(id);

    public async Task<ActionResponse<Purchase>> UpdateAsync(Purchase modelo) => await _purchaseService.UpdateAsync(modelo);

    public async Task<ActionResponse<Purchase>> AddAsync(Purchase modelo, string email) => await _purchaseService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _purchaseService.DeleteAsync(id);
}