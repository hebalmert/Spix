using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class NodeMapServiceX : INodeMapServiceX
{
    private readonly INodeMapService _nodeMapService;

    public NodeMapServiceX(INodeMapService nodeMapService)
    {
        _nodeMapService = nodeMapService;
    }

    public async Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboNodesAsync(string username) => await _nodeMapService.ComboNodesAsync(username);

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboViewsAsync() => await _nodeMapService.ComboViewsAsync();

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCoveragesAsync() => await _nodeMapService.ComboCoveragesAsync();

    public async Task<ActionResponse<NodeMapDto>> GetAsync(Guid nodeId, string username) => await _nodeMapService.GetAsync(nodeId, username);
}
