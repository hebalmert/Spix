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

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetActiveSummaryAsync(int stateId, string username)
    {
        return await _reportService.GetActiveSummaryAsync(stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetActiveContractsAsync(int stateId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetActiveContractsAsync(stateId, pagination, username);
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

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetZoneSummaryAsync(Guid zoneId, int stateId, string username)
    {
        return await _reportService.GetZoneSummaryAsync(zoneId, stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetZoneContractsAsync(Guid zoneId, int stateId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetZoneContractsAsync(zoneId, stateId, pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboContractStatesAsync(string username)
    {
        return await _reportService.ComboContractStatesAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboNodesAsync(string username)
    {
        return await _reportService.ComboNodesAsync(username);
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboServersAsync(string username)
    {
        return await _reportService.ComboServersAsync(username);
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetNodeSummaryAsync(Guid nodeId, int stateId, string username)
    {
        return await _reportService.GetNodeSummaryAsync(nodeId, stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetNodeContractsAsync(Guid nodeId, int stateId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetNodeContractsAsync(nodeId, stateId, pagination, username);
    }

    public async Task<ActionResponse<ReportActiveSummaryDto>> GetServerSummaryAsync(Guid serverId, int stateId, string username)
    {
        return await _reportService.GetServerSummaryAsync(serverId, stateId, username);
    }

    public async Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetServerContractsAsync(Guid serverId, int stateId, PaginationDTO pagination, string username)
    {
        return await _reportService.GetServerContractsAsync(serverId, stateId, pagination, username);
    }
}
