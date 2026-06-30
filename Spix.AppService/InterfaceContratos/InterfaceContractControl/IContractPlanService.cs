using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractPlanService
{
    Task<ActionResponse<ContractPlan>> GetAsync(Guid id);

    Task<ActionResponse<ContractPlan>> AddAsync(ContractPlan modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
