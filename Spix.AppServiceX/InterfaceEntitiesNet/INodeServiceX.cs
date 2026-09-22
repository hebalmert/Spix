using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceEntitiesNet;

public interface INodeServiceX
{
    Task<ActionResponse<IEnumerable<Node>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<NodeListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Node>> GetAsync(Guid id, string username, bool withCredentials);

    Task<ActionResponse<Node>> UpdateAsync(Node modelo, string username);

    Task<ActionResponse<Node>> AddAsync(Node modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
