using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
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

    public async Task<ActionResponse<IEnumerable<ContractClient>>> GetControlContratos(PaginationDTO pagination, string email) => await _contractClientService.GetControlContratos(pagination, email);

    public async Task<ActionResponse<IEnumerable<ContractClient>>> GetAsync(PaginationDTO pagination, string email) => await _contractClientService.GetAsync(pagination, email);

    public async Task<ActionResponse<ContractClient>> GetAsync(Guid id) => await _contractClientService.GetAsync(id);

    public async Task<ActionResponse<ContractClient>> GetProcesandoAsync(Guid id) => await _contractClientService.GetProcesandoAsync(id);

    public async Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo) => await _contractClientService.UpdateAsync(modelo);

    public async Task<ActionResponse<ContractClient>> AddAsync(ContractClient modelo, string email) => await _contractClientService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _contractClientService.DeleteAsync(id);
}