using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class ReportServiceX : IReportServiceX
{
    private readonly IReportService _reportService;

    public ReportServiceX(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetActiveSummaryAsync(string username)
    {
        return await _reportService.GetActiveSummaryAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetActiveContractsAsync(PaginationDTO pagination, string username)
    {
        return await _reportService.GetActiveContractsAsync(pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatesAsync(string username)
    {
        return await _reportService.ComboStatesAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCitiesAsync(int stateId, string username)
    {
        return await _reportService.ComboCitiesAsync(stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboZonesAsync(int cityId, string username)
    {
        return await _reportService.ComboZonesAsync(cityId, username);
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetZoneSummaryAsync(Guid zoneId, string username)
    {
        return await _reportService.GetZoneSummaryAsync(zoneId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetZoneContractsAsync(Guid zoneId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetZoneContractsAsync(zoneId, pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboNodesAsync(string username)
    {
        return await _reportService.ComboNodesAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboServersAsync(string username)
    {
        return await _reportService.ComboServersAsync(username);
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetNodeSummaryAsync(Guid nodeId, string username)
    {
        return await _reportService.GetNodeSummaryAsync(nodeId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetNodeContractsAsync(Guid nodeId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetNodeContractsAsync(nodeId, pagination, username);
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetServerSummaryAsync(Guid serverId, string username)
    {
        return await _reportService.GetServerSummaryAsync(serverId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetServerContractsAsync(Guid serverId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetServerContractsAsync(serverId, pagination, username);
    }
}
