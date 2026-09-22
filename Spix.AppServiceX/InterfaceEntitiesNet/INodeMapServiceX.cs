using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceEntitiesNet;

public interface INodeMapServiceX
{
    Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboNodesAsync(string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboViewsAsync();

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboCoveragesAsync();

    Task<ActionResponse<NodeMapDto>> GetAsync(Guid nodeId, string username);
}
