using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractSuspendedService
{
    Task<ActionResponse<bool>> SuspendAsync(Guid contractClientId, string? motivo, string username);

    Task<ActionResponse<SuspendedListDTO>> GetRecordsAsync(string? filter, DateTime? desde, DateTime? hasta, bool soloAbiertas, string username);

    Task<ActionResponse<IEnumerable<ActiveContractDTO>>> SearchActiveAsync(string filter, string username);

    Task<ActionResponse<IEnumerable<ContractSuspendedDTO>>> SearchAsync(string filter, string username);

    Task<ActionResponse<ContractSuspendedDTO>> ActivateAsync(Guid id, string username);
}
