using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractSuspendedAuditServiceX
{
    Task<ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>> GetAsync(
        DateTime startDate,
        DateTime endDate,
        string username);
}
