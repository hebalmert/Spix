using Spix.AppService.InterfacesInven;
using Spix.AppServiceX.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementInven;

public class ReportInventoryServiceX : IReportInventoryServiceX
{
    private readonly IReportInventoryService _reportInventoryService;

    public ReportInventoryServiceX(IReportInventoryService reportInventoryService)
    {
        _reportInventoryService = reportInventoryService;
    }

    public async Task<ActionResponse<ReportSerialSummaryDto>> GetSerialSummaryAsync(string username)
    {
        return await _reportInventoryService.GetSerialSummaryAsync(username);
    }

    public ActionResponse<IEnumerable<IntItemModel>> SerialStatesCombo() => _reportInventoryService.SerialStatesCombo();

    public async Task<ActionResponse<IEnumerable<ReportSerialDetailDto>>> GetSerialDetailAsync(string username, SerialStateType? estado, Guid? productId, Guid? storageId)
        => await _reportInventoryService.GetSerialDetailAsync(username, estado, productId, storageId);

    public async Task<ActionResponse<IEnumerable<ReportSerialDto>>> GetSerialsAsync(string username)
    {
        return await _reportInventoryService.GetSerialsAsync(username);
    }
}
