using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface IHotSpotTypeService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<HotSpotType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<HotSpotType>> GetAsync(int id);

    Task<ActionResponse<HotSpotType>> UpdateAsync(HotSpotType modelo);

    Task<ActionResponse<HotSpotType>> AddAsync(HotSpotType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}