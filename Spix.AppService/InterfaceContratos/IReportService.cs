using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IReportService
{
    Task<ActionResponse<ReportActiveSummaryDto>> GetActiveSummaryAsync(int stateId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetActiveContractsAsync(int stateId, PaginationDTO pagination, string username);

    //El reporte por zona pregunta estado, ciudad y zona
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatesAsync(string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCitiesAsync(int stateId, string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboZonesAsync(int cityId, string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetZoneSummaryAsync(Guid zoneId, int stateId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetZoneContractsAsync(Guid zoneId, int stateId, PaginationDTO pagination, string username);

    //Cada reporte de detalle se filtra por lo que se elija: zona, AP o servidor
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboContractStatesAsync(string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboNodesAsync(string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboServersAsync(string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetNodeSummaryAsync(Guid nodeId, int stateId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetNodeContractsAsync(Guid nodeId, int stateId, PaginationDTO pagination, string username);

    Task<ActionResponse<ReportActiveSummaryDto>> GetServerSummaryAsync(Guid serverId, int stateId, string username);

    Task<ActionResponse<IEnumerable<ReportActiveContractDto>>> GetServerContractsAsync(Guid serverId, int stateId, PaginationDTO pagination, string username);
}
