using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IChainTypesUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<ChainType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<ChainType>> GetAsync(int id);

    Task<ActionResponse<ChainType>> UpdateAsync(ChainType modelo);

    Task<ActionResponse<ChainType>> AddAsync(ChainType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}