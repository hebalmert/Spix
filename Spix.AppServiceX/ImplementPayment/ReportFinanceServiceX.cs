using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementPayment;

public class ReportFinanceServiceX : IReportFinanceServiceX
{
    private readonly IReportFinanceService _reportFinanceService;

    public ReportFinanceServiceX(IReportFinanceService reportFinanceService)
    {
        _reportFinanceService = reportFinanceService;
    }

    public async Task<ActionResponse<ReportCollectionSummaryDto>> GetCollectionSummaryAsync(PaginationDTO pagination, string username)
    {
        return await _reportFinanceService.GetCollectionSummaryAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportCollectorDto>>> GetCollectorsAsync(PaginationDTO pagination, string username)
    {
        return await _reportFinanceService.GetCollectorsAsync(pagination, username);
    }

    public async Task<ActionResponse<ReportNotesSummaryDto>> GetNotesSummaryAsync(PaginationDTO pagination, string username)
    {
        return await _reportFinanceService.GetNotesSummaryAsync(pagination, username);
    }

    public async Task<ActionResponse<ReportAgingDto>> GetAgingAsync(string username)
    {
        return await _reportFinanceService.GetAgingAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<ReportDebtorDto>>> GetTopDebtorsAsync(int top, string username)
    {
        return await _reportFinanceService.GetTopDebtorsAsync(top, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportContractorCommissionDto>>> GetContractorCommissionsAsync(PaginationDTO pagination, string username)
    {
        return await _reportFinanceService.GetContractorCommissionsAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportAuditDto>>> GetAuditAsync(int eventType, PaginationDTO pagination, string username)
    {
        return await _reportFinanceService.GetAuditAsync(eventType, pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboEventTypesAsync(string username)
    {
        return await _reportFinanceService.ComboEventTypesAsync(username);
    }
}
