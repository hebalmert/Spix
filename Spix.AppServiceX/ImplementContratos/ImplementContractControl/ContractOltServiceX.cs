using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos.ImplementContractControl;

public class ContractOltServiceX : IContractOltServiceX
{
    private readonly IContractOltService _contractService;

    public ContractOltServiceX(IContractOltService contractService)
    {
        _contractService = contractService;
    }

    public async Task<ActionResponse<ContractOlt>> GetAsync(Guid id, string username) => await _contractService.GetAsync(id, username);

    public async Task<ActionResponse<ContractOlt>> AddAsync(ContractOlt modelo, string username) => await _contractService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _contractService.DeleteAsync(id, username);
}
