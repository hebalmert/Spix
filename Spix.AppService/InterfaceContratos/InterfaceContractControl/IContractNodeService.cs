using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractNodeService
{
    Task<ActionResponse<ContractNode>> GetAsync(Guid id);

    Task<ActionResponse<ContractNode>> AddAsync(ContractNode modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
