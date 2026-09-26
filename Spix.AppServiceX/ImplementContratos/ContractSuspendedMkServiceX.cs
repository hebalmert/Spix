using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractSuspendedMkServiceX : IContractSuspendedMkServiceX
{
    private readonly IContractSuspendedMkService _contractSuspendedMkService;

    public ContractSuspendedMkServiceX(IContractSuspendedMkService contractSuspendedMkService)
    {
        _contractSuspendedMkService = contractSuspendedMkService;
    }

    public async Task<ActionResponse<SuspendMkSetupDTO>> GetSuspendSetupAsync(Guid contractClientId, string username)
        => await _contractSuspendedMkService.GetSuspendSetupAsync(contractClientId, username);

    public async Task<ActionResponse<ReactivateMkSetupDTO>> GetReactivateSetupAsync(Guid contractClientId, string username)
        => await _contractSuspendedMkService.GetReactivateSetupAsync(contractClientId, username);

    public async Task<ActionResponse<bool>> SuspendSaveAsync(Guid contractClientId, string? motivo, string username)
        => await _contractSuspendedMkService.SuspendSaveAsync(contractClientId, motivo, username);

    public async Task<ActionResponse<bool>> ReactivateSaveAsync(Guid contractClientId, string username)
        => await _contractSuspendedMkService.ReactivateSaveAsync(contractClientId, username);
}
