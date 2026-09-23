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

    Task<ActionResponse<CorteSummaryDto>> GetSummaryAsync(string username);

    //El corte se hace en tres pasos: se revisa, se corta por lotes y se cierra
    Task<ActionResponse<CorteCheckDto>> CheckAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<CorteDetailDto>>> GetDetailsAsync(Guid id, PaginationDTO pagination, string username);

    Task<ActionResponse<CorteRunResultDto>> RunServerAsync(Guid id, Guid serverId, string username);

    Task<ActionResponse<RunSuspended>> FinishRunAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username);
}
