using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractClientServiceX : IContractClientServiceX
{
    private readonly IContractClientService _contractClientService;

    public ContractClientServiceX(IContractClientService contractClientService)
    {
        _contractClientService = contractClientService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatusAsync() => await _contractClientService.GetComboStatusAsync();

    public async Task<ActionResponse<IEnumerable<ContractClient>>> GetAsync(PaginationDTO pagination, string username) => await _contractClientService.GetAsync(pagination, username);

    public async Task<ActionResponse<ContractClient>> GetAsync(Guid id) => await _contractClientService.GetAsync(id);

    public async Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo) => await _contractClientService.UpdateAsync(modelo);

    public async Task<ActionResponse<ContractClient>> AddAsync(ContractClient modelo, string username) => await _contractClientService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractClientService.DeleteAsync(id);
}