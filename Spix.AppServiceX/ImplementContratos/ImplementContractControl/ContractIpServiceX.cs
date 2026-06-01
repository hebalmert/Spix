using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractIpServiceX : IContractIpServiceX
{
    private readonly IContractIpService _contractService;

    public ContractIpServiceX(IContractIpService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractIp>> GetAsync(Guid id) => await _contractService.GetAsync(id);

    public async Task<ActionResponse<ContractIp>> AddAsync(ContractIp modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractService.DeleteAsync(id);
}
