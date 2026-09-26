using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractMkSetupServiceX
{
    Task<ActionResponse<ContractQueSetupDTO>> GetQueSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractBindSetupDTO>> GetBindSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractQue>> SaveQueAsync(ContractQueSaveDTO datos, string username);

    Task<ActionResponse<bool>> RemoveQueAsync(ContractQueRemoveDTO datos, string username);

    Task<ActionResponse<ContractBind>> SaveBindAsync(ContractBind modelo, string username);

    Task<ActionResponse<bool>> RemoveBindAsync(Guid id, string username);

    Task<ActionResponse<ContractQueRemoveSetupDTO>> GetQueRemoveSetupAsync(Guid contractQueId, string username);

    Task<ActionResponse<ContractMkConnectionDTO>> GetConnectionAsync(Guid contractClientId, string username);
}
