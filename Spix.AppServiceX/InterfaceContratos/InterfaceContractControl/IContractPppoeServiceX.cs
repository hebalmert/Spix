using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractPppoeServiceX
{
    Task<ActionResponse<ContractPppoe>> GetAsync(Guid id, string username);

    ActionResponse<IEnumerable<IntItemModel>> AccessStatesCombo();

    Task<ActionResponse<ContractPppoe>> AddAsync(ContractPppoe modelo, string username);

    Task<ActionResponse<ContractPppoe>> UpdateAsync(ContractPppoe modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
