using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractServerServiceX
{
    Task<ActionResponse<ContractServer>> GetAsync(Guid id);

    Task<ActionResponse<ContractServer>> AddAsync(ContractServer modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
