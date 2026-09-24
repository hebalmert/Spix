using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesPayment;

public interface IReportFinanceService
{
    Task<ActionResponse<ReportCollectionSummaryDto>> GetCollectionSummaryAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportCollectorDto>>> GetCollectorsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ReportNotesSummaryDto>> GetNotesSummaryAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ReportAgingDto>> GetAgingAsync(string username);

    Task<ActionResponse<IEnumerable<ReportDebtorDto>>> GetTopDebtorsAsync(int top, string username);

    Task<ActionResponse<IEnumerable<ReportContractorCommissionDto>>> GetContractorCommissionsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportAuditDto>>> GetAuditAsync(int eventType, PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboEventTypesAsync(string username);
}
