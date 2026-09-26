using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IActivationMkServiceX
{
    Task<ActionResponse<ActivationMkSetupDTO>> GetActivateSetupAsync(Guid serverId, string username);

    Task<ActionResponse<ActivationRunResultDto>> ActivateSaveAsync(Guid serverId, List<Guid> activados, string username);
}
