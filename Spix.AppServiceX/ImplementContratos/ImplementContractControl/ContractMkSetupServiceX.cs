using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractMkSetupServiceX : IContractMkSetupServiceX
{
    private readonly IContractMkSetupService _contractService;

    public ContractMkSetupServiceX(IContractMkSetupService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractQueSetupDTO>> GetQueSetupAsync(Guid contractClientId, string username)
        => await _contractService.GetQueSetupAsync(contractClientId, username);

    public async Task<ActionResponse<ContractBindSetupDTO>> GetBindSetupAsync(Guid contractClientId, string username)
        => await _contractService.GetBindSetupAsync(contractClientId, username);

    public async Task<ActionResponse<ContractQue>> SaveQueAsync(ContractQueSaveDTO datos, string username)
        => await _contractService.SaveQueAsync(datos, username);

    public async Task<ActionResponse<bool>> RemoveQueAsync(ContractQueRemoveDTO datos, string username)
        => await _contractService.RemoveQueAsync(datos, username);

    public async Task<ActionResponse<ContractBind>> SaveBindAsync(ContractBind modelo, string username)
        => await _contractService.SaveBindAsync(modelo, username);

    public async Task<ActionResponse<bool>> RemoveBindAsync(Guid id, string username)
        => await _contractService.RemoveBindAsync(id, username);

    public async Task<ActionResponse<ContractQueRemoveSetupDTO>> GetQueRemoveSetupAsync(Guid contractQueId, string username)
        => await _contractService.GetQueRemoveSetupAsync(contractQueId, username);

    public async Task<ActionResponse<ContractMkConnectionDTO>> GetConnectionAsync(Guid contractClientId, string username)
        => await _contractService.GetConnectionAsync(contractClientId, username);
}
