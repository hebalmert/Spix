using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IOperationUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Operation>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Operation>> GetAsync(int id);

    Task<ActionResponse<Operation>> UpdateAsync(Operation modelo);

    Task<ActionResponse<Operation>> AddAsync(Operation modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}