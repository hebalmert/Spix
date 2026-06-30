using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractPlanServiceX : IContractPlanServiceX
{
    private readonly IContractPlanService _contractService;

    public ContractPlanServiceX(IContractPlanService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractPlan>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractPlan>> AddAsync(ContractPlan modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
