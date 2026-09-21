using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractAuditServiceX : IContractAuditServiceX
{
    private readonly IContractAuditService _contractAuditService;

    public ContractAuditServiceX(IContractAuditService contractAuditService)
    {
        _contractAuditService = contractAuditService;
    }

    public async Task<ActionResponse<ContractAuditListDTO>> GetAsync(Guid contractClientId, string username) =>
        await _contractAuditService.GetAsync(contractClientId, username);
}
