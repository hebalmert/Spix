using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractIpServiceX
{
    Task<ActionResponse<ContractIp>> GetAsync(Guid id);

    Task<ActionResponse<ContractIp>> AddAsync(ContractIp modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
