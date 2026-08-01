using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractSuspendedAuditServiceX : IContractSuspendedAuditServiceX
{
    private readonly IContractSuspendedAuditService _contractSuspendedAuditService;

    public ContractSuspendedAuditServiceX(IContractSuspendedAuditService contractSuspendedAuditService)
    {
        _contractSuspendedAuditService = contractSuspendedAuditService;
    }

    public async Task<ActionResponse<IEnumerable<ContractSuspendedAuditDTO>>> GetAsync(
        DateTime startDate,
        DateTime endDate,
        string username)
    {
        return await _contractSuspendedAuditService.GetAsync(startDate, endDate, username);
    }
}
