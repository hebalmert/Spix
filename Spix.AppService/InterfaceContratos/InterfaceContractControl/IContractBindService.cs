using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractBindService
{
    Task<ActionResponse<ContractBind>> GetAsync(Guid id);

    Task<ActionResponse<ContractBind>> AddAsync(ContractBind modelo, string username);

    Task<ActionResponse<ContractBind>> UpdateAsync(ContractBind modelo);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
