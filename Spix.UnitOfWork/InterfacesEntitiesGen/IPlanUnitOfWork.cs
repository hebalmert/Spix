using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IPlanUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboUpAsync();

    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboDownAsync();

    Task<ActionResponse<IEnumerable<Plan>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Plan>> GetAsync(Guid id);

    Task<ActionResponse<Plan>> UpdateAsync(Plan modelo);

    Task<ActionResponse<Plan>> AddAsync(Plan modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}