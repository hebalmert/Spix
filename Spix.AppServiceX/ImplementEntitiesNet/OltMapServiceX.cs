using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class OltMapServiceX : IOltMapServiceX
{
    private readonly IOltMapService _oltMapService;

    public OltMapServiceX(IOltMapService oltMapService)
    {
        _oltMapService = oltMapService;
    }

    public async Task<ActionResponse<IEnumerable<GuidNameModel>>> ComboOltsAsync(string username) => await _oltMapService.ComboOltsAsync(username);

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboViewsAsync() => await _oltMapService.ComboViewsAsync();

    public async Task<ActionResponse<IEnumerable<OltMapItemDto>>> GetAllAsync(string username) => await _oltMapService.GetAllAsync(username);

    public async Task<ActionResponse<OltMapDto>> GetAsync(Guid oltId, string username) => await _oltMapService.GetAsync(oltId, username);
}
