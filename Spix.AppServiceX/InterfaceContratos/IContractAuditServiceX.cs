using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractAuditServiceX
{
    Task<ActionResponse<ContractAuditListDTO>> GetAsync(Guid contractClientId, string username);
}
