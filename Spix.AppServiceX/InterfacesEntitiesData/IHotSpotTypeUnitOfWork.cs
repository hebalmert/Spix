using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IHotSpotTypeUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<HotSpotType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<HotSpotType>> GetAsync(int id);

    Task<ActionResponse<HotSpotType>> UpdateAsync(HotSpotType modelo);

    Task<ActionResponse<HotSpotType>> AddAsync(HotSpotType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}