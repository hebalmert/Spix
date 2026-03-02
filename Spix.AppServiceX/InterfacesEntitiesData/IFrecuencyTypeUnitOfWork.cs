using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IFrecuencyTypeUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<FrecuencyType>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<FrecuencyType>> GetAsync(int id);

    Task<ActionResponse<FrecuencyType>> UpdateAsync(FrecuencyType modelo);

    Task<ActionResponse<FrecuencyType>> AddAsync(FrecuencyType modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}