using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractSuspendedService
{
    Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username);

    Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username);
}
