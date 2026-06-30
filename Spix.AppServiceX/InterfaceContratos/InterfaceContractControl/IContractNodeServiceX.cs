using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractNodeServiceX
{
    Task<ActionResponse<ContractNode>> GetAsync(Guid id);

    Task<ActionResponse<ContractNode>> AddAsync(ContractNode modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
