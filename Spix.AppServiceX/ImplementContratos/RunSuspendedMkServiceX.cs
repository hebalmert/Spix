using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class RunSuspendedMkServiceX : IRunSuspendedMkServiceX
{
    private readonly IRunSuspendedMkService _runSuspendedMkService;

    public RunSuspendedMkServiceX(IRunSuspendedMkService runSuspendedMkService)
    {
        _runSuspendedMkService = runSuspendedMkService;
    }

    public async Task<ActionResponse<CorteMkSetupDTO>> GetRunSetupAsync(Guid id, Guid serverId, string username)
        => await _runSuspendedMkService.GetRunSetupAsync(id, serverId, username);

    public async Task<ActionResponse<CorteRunResultDto>> RunSaveAsync(Guid id, Guid serverId, List<Guid> suspendidos, string username)
        => await _runSuspendedMkService.RunSaveAsync(id, serverId, suspendidos, username);
}
