using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class RunSuspendedServiceX : IRunSuspendedServiceX
{
    private readonly IRunSuspendedService _runSuspendedService;

    public RunSuspendedServiceX(IRunSuspendedService runSuspendedService)
    {
        _runSuspendedService = runSuspendedService;
    }

    public async Task<ActionResponse<IEnumerable<RunSuspended>>> GetAsync(PaginationDTO pagination, string username)
    {
        return await _runSuspendedService.GetAsync(pagination, username);
    }

    public async Task<ActionResponse<RunSuspended>> GetByIdAsync(Guid id, string username)
    {
        return await _runSuspendedService.GetByIdAsync(id, username);
    }

    public async Task<ActionResponse<RunSuspended>> AddAsync(RunSuspended model, string username)
    {
        return await _runSuspendedService.AddAsync(model, username);
    }

    public async Task<ActionResponse<RunSuspended>> UpdateAsync(RunSuspended model, string username)
    {
        return await _runSuspendedService.UpdateAsync(model, username);
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        return await _runSuspendedService.DeleteAsync(id, username);
    }

    public async Task<ActionResponse<RunSuspended>> RunAsync(Guid id, string username)
    {
        return await _runSuspendedService.RunAsync(id, username);
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username)
    {
        return await _runSuspendedService.ComboMonthsAsync(username);
    }
}
