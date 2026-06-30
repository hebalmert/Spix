using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractQueServiceX : IContractQueServiceX
{
    private readonly IContractQueService _contractService;

    public ContractQueServiceX(IContractQueService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractQue>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractQue>> AddAsync(ContractQue modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
