using Spix.AppService.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class HotSpotTypeServiceX : IHotSpotTypeServiceX
{
    private readonly IHotSpotTypeService _hotSpotTypeService;

    public HotSpotTypeServiceX(IHotSpotTypeService hotSpotTypeService)
    {
        _hotSpotTypeService = hotSpotTypeService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _hotSpotTypeService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<HotSpotType>>> GetAsync(PaginationDTO pagination) => await _hotSpotTypeService.GetAsync(pagination);

    public async Task<ActionResponse<HotSpotType>> GetAsync(int id) => await _hotSpotTypeService.GetAsync(id);

    public async Task<ActionResponse<HotSpotType>> UpdateAsync(HotSpotType modelo) => await _hotSpotTypeService.UpdateAsync(modelo);

    public async Task<ActionResponse<HotSpotType>> AddAsync(HotSpotType modelo) => await _hotSpotTypeService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _hotSpotTypeService.DeleteAsync(id);
}