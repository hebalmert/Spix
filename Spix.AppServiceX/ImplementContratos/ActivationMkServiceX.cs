using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ActivationMkServiceX : IActivationMkServiceX
{
    private readonly IActivationMkService _activationMkService;

    public ActivationMkServiceX(IActivationMkService activationMkService)
    {
        _activationMkService = activationMkService;
    }

    public async Task<ActionResponse<ActivationMkSetupDTO>> GetActivateSetupAsync(Guid serverId, string username)
        => await _activationMkService.GetActivateSetupAsync(serverId, username);

    public async Task<ActionResponse<ActivationRunResultDto>> ActivateSaveAsync(Guid serverId, List<Guid> activados, string username)
        => await _activationMkService.ActivateSaveAsync(serverId, activados, username);
}
