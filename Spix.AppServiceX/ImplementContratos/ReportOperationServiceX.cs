using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class ReportOperationServiceX : IReportOperationServiceX
{
    private readonly IReportOperationService _reportOperationService;

    public ReportOperationServiceX(IReportOperationService reportOperationService)
    {
        _reportOperationService = reportOperationService;
    }

    public async Task<ActionResponse<ReportContractsSummaryDto>> GetContractsSummaryAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetContractsSummaryAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportPlanDto>>> GetTopPlansAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetTopPlansAsync(pagination, username);
    }

    public async Task<ActionResponse<ReportServicesSummaryDto>> GetServicesSummaryAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetServicesSummaryAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportServiceDto>>> GetTopServicesAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetTopServicesAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportTechnicianDto>>> GetTopTechniciansAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetTopTechniciansAsync(pagination, username);
    }

    public async Task<ActionResponse<ReportCutOffSummaryDto>> GetCutOffSummaryAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetCutOffSummaryAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportCutOffZoneDto>>> GetCutOffZonesAsync(PaginationDTO pagination, string username)
    {
        return await _reportOperationService.GetCutOffZonesAsync(pagination, username);
    }

    public async Task<ActionResponse<ReportChurnDto>> GetChurnSummaryAsync(string username)
    {
        return await _reportOperationService.GetChurnSummaryAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<ReportChurnZoneDto>>> GetChurnZonesAsync(string username)
    {
        return await _reportOperationService.GetChurnZonesAsync(username);
    }
}
