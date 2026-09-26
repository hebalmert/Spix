using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IRunSuspendedMkServiceX
{
    Task<ActionResponse<CorteMkSetupDTO>> GetRunSetupAsync(Guid id, Guid serverId, string username);

    Task<ActionResponse<CorteRunResultDto>> RunSaveAsync(Guid id, Guid serverId, List<Guid> suspendidos, string username);
}
