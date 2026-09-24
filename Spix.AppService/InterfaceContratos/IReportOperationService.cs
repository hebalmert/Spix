using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IReportOperationService
{
    //Contratos instalados y servicios del periodo: son dos reportes distintos
    Task<ActionResponse<ReportContractsSummaryDto>> GetContractsSummaryAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportPlanDto>>> GetTopPlansAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ReportServicesSummaryDto>> GetServicesSummaryAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportServiceDto>>> GetTopServicesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportTechnicianDto>>> GetTopTechniciansAsync(PaginationDTO pagination, string username);

    //Corte de servicio y bajas: como respondio la gente al corte y quien ya no esta
    Task<ActionResponse<ReportCutOffSummaryDto>> GetCutOffSummaryAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ReportCutOffZoneDto>>> GetCutOffZonesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ReportChurnDto>> GetChurnSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ReportChurnZoneDto>>> GetChurnZonesAsync(string username);
}
