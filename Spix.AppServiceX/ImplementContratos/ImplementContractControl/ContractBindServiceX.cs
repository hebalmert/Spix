using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractBindServiceX : IContractBindServiceX
{
    private readonly IContractBindService _contractService;

    public ContractBindServiceX(IContractBindService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractBind>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractBind>> AddAsync(ContractBind modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<ContractBind>> UpdateAsync(ContractBind modelo) => await _contractService.UpdateAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
