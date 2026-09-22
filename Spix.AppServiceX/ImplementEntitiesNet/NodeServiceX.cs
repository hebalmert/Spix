using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class NodeServiceX : INodeServiceX
{
    private readonly INodeService _nodeService;

    public NodeServiceX(INodeService nodeService)
    {
        _nodeService = nodeService;
    }

    public async Task<ActionResponse<IEnumerable<Node>>> ComboAsync(string username, Guid? id = null) => await _nodeService.ComboAsync(username, id);

    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username) => await _nodeService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<NodeListItemDto>>> GetAsync(PaginationDTO pagination, string username) => await _nodeService.GetAsync(pagination, username);

    public async Task<ActionResponse<Node>> GetAsync(Guid id, string username, bool withCredentials) => await _nodeService.GetAsync(id, username, withCredentials);

    public async Task<ActionResponse<Node>> UpdateAsync(Node modelo, string username) => await _nodeService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Node>> AddAsync(Node modelo, string username) => await _nodeService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _nodeService.DeleteAsync(id, username);
}
