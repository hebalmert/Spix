using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractMacServiceX
{
    Task<ActionResponse<ContractMac>> GetAsync(Guid id);

    Task<ActionResponse<ContractMac>> AddAsync(ContractMac modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
