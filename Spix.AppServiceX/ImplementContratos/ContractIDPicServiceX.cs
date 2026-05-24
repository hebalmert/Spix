using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractIDPicServiceX : IContractIDPicServiceX
{
    private readonly IContractIDPicService _contractIDPicService;

    public ContractIDPicServiceX(IContractIDPicService contractIDPicService)
    {
        _contractIDPicService = contractIDPicService;
    }

    public async Task<ActionResponse<ContractIDPic>> GetAsync(Guid id) => await _contractIDPicService.GetAsync(id);

    public async Task<ActionResponse<ContractIDPic>> UpdateAsync(ContractIDPic modelo) => await _contractIDPicService.UpdateAsync(modelo);

    public async Task<ActionResponse<ContractIDPic>> AddAsync(ContractIDPic modelo, string username) => await _contractIDPicService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractIDPicService.DeleteAsync(id);
}
