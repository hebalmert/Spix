using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractPppoeServiceX : IContractPppoeServiceX
{
    private readonly IContractPppoeService _contractService;

    public ContractPppoeServiceX(IContractPppoeService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractPppoe>> GetAsync(Guid id, string username) => await _contractService.GetAsync(id, username);

    public ActionResponse<IEnumerable<IntItemModel>> AccessStatesCombo() => _contractService.AccessStatesCombo();

    public async Task<ActionResponse<ContractPppoe>> AddAsync(ContractPppoe modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<ContractPppoe>> UpdateAsync(ContractPppoe modelo, string username) => await _contractService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _contractService.DeleteAsync(id, username);
}
