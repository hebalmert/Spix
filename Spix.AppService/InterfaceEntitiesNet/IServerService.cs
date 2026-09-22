using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntitiesNet;

public interface IServerService
{
    Task<ActionResponse<IEnumerable<Server>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ServerListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Server>> GetAsync(Guid id, string username, bool withCredentials);

    Task<ActionResponse<Server>> UpdateAsync(Server modelo, string username);

    Task<ActionResponse<Server>> AddAsync(Server modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
