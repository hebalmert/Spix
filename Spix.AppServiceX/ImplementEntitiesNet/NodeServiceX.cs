using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
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

    public async Task<ActionResponse<IEnumerable<Node>>> GetAsync(PaginationDTO pagination, string email) => await _nodeService.GetAsync(pagination, email);

    public async Task<ActionResponse<Node>> GetAsync(Guid id) => await _nodeService.GetAsync(id);

    public async Task<ActionResponse<Node>> UpdateAsync(Node modelo) => await _nodeService.UpdateAsync(modelo);

    public async Task<ActionResponse<Node>> AddAsync(Node modelo, string email) => await _nodeService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _nodeService.DeleteAsync(id);
}