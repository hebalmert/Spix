using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractMapServiceX : IContractMapServiceX
{
    private readonly IContractMapService _contractService;

    public ContractMapServiceX(IContractMapService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractMap>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractMap>> AddAsync(ContractMap modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<ContractMap>> UpdateAsync(ContractMap modelo) => await _contractService.UpdateAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
