using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceEntitiesNet;

public interface IOltMapServiceX
{
    Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboOltsAsync(string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboViewsAsync();

    Task<ActionResponse<IEnumerable<OltMapItemDto>>> GetAllAsync(string username);

    Task<ActionResponse<OltMapDto>> GetAsync(Guid oltId, string username);
}
