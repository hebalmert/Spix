using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesData;
using Spix.UnitOfWork.InterfacesEntitiesData;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class HotSpotTypeUnitOfWork : IHotSpotTypeUnitOfWork
{
    private readonly IHotSpotTypeService _hotSpotTypeService;

    public HotSpotTypeUnitOfWork(IHotSpotTypeService hotSpotTypeService)
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