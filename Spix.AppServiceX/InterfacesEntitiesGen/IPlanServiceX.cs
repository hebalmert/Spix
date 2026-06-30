using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesEntitiesGen;

public interface IPlanServiceX
{
    Task<ActionResponse<IEnumerable<Plan>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<IEnumerable<Plan>>> ComboByCategoryAsync(string username, Guid planCategoryId, Guid? id = null);

    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboUpAsync();

    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboDownAsync();

    Task<ActionResponse<IEnumerable<Plan>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Plan>> GetAsync(Guid id);

    Task<ActionResponse<Plan>> UpdateAsync(Plan modelo);

    Task<ActionResponse<Plan>> AddAsync(Plan modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
