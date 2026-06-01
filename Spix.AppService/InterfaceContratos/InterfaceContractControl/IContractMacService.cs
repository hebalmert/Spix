using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractMacService
{
    Task<ActionResponse<ContractMac>> GetAsync(Guid id);

    Task<ActionResponse<ContractMac>> AddAsync(ContractMac modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
