using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractServerService
{
    Task<ActionResponse<ContractServer>> GetAsync(Guid id);

    Task<ActionResponse<ContractServer>> AddAsync(ContractServer modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
