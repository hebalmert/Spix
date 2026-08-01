using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IRunSuspendedServiceX
{
    Task<ActionResponse<IEnumerable<RunSuspended>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<RunSuspended>> GetByIdAsync(Guid id, string username);

    Task<ActionResponse<RunSuspended>> AddAsync(RunSuspended model, string username);

    Task<ActionResponse<RunSuspended>> UpdateAsync(RunSuspended model, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);

    Task<ActionResponse<RunSuspended>> RunAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username);
}
