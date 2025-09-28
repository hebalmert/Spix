using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

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