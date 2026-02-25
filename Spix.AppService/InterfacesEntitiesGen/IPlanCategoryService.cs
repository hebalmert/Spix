using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesGen;

public interface IPlanCategoryService
{
    Task<ActionResponse<IEnumerable<PlanCategory>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<PlanCategory>> GetAsync(Guid id);

    Task<ActionResponse<PlanCategory>> UpdateAsync(PlanCategory modelo);

    Task<ActionResponse<PlanCategory>> AddAsync(PlanCategory modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}