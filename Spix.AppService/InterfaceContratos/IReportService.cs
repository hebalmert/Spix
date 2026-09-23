using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IReportService
{
    Task<ActionResponse<ReportActiveSummaryDto>> GetActiveSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetActiveContractsAsync(PaginationDTO pagination, string username);

    //El reporte por zona pregunta estado, ciudad y zona
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatesAsync(string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCitiesAsync(int stateId, string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboZonesAsync(int cityId, string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetZoneSummaryAsync(Guid zoneId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetZoneContractsAsync(Guid zoneId, PaginationDTO pagination, string username);

    //Cada reporte de detalle se filtra por lo que se elija: zona, AP o servidor
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboNodesAsync(string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboServersAsync(string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetNodeSummaryAsync(Guid nodeId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetNodeContractsAsync(Guid nodeId, PaginationDTO pagination, string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetServerSummaryAsync(Guid serverId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetServerContractsAsync(Guid serverId, PaginationDTO pagination, string username);
}
