using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractSuspendedAuditService
{
    Task<ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>> GetAsync(
        DateTime startDate,
        DateTime endDate,
        string username);
}
