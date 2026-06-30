using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractBindServiceX
{
    Task<ActionResponse<ContractBind>> GetAsync(Guid id);

    Task<ActionResponse<ContractBind>> AddAsync(ContractBind modelo, string username);

    Task<ActionResponse<ContractBind>> UpdateAsync(ContractBind modelo);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
