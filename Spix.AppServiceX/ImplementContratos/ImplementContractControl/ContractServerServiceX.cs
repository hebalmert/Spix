using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractServerServiceX : IContractServerServiceX
{
    private readonly IContractServerService _contractService;

    public ContractServerServiceX(IContractServerService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractServer>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractServer>> AddAsync(ContractServer modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
