using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractNodeServiceX : IContractNodeServiceX
{
    private readonly IContractNodeService _contractService;

    public ContractNodeServiceX(IContractNodeService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractNode>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractNode>> AddAsync(ContractNode modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
