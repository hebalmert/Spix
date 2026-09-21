using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractAuditService
{
    Task<ActionResponse<ContractAuditListDTO>> GetAsync(Guid contractClientId, string username);
}
