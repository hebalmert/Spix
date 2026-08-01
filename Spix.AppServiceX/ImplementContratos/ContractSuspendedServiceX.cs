using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractSuspendedServiceX : IContractSuspendedServiceX
{
    private readonly IContractSuspendedService _contractSuspendedService;

    public ContractSuspendedServiceX(IContractSuspendedService contractSuspendedService)
    {
        _contractSuspendedService = contractSuspendedService;
    }

    public async Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username)
    {
        return await _contractSuspendedService.SearchAsync(filter, username);
    }

    public async Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username)
    {
        return await _contractSuspendedService.ActivateAsync(id, username);
    }
}
