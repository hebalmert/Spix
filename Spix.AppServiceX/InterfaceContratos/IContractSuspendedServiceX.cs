using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractSuspendedServiceX
{
    Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username);

    Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username);
}
