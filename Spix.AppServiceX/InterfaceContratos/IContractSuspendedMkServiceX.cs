using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractSuspendedMkServiceX
{
    Task<ActionResponse<SuspendMkSetupDTO>> GetSuspendSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<ReactivateMkSetupDTO>> GetReactivateSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<bool>> SuspendSaveAsync(Guid contractClientId, string? motivo, string username);

    Task<ActionResponse<bool>> ReactivateSaveAsync(Guid contractClientId, string username);
}
