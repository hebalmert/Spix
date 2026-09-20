using Spix.AppService.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementContratos;

public class ContractControlServiceX : IContractControlServiceX
{
    private readonly IContractControlService _contractControlService;

    public ContractControlServiceX(IContractControlService contractControlService)
    {
        _contractControlService = contractControlService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetStateChangeOptionsAsync(Guid contractClientId, string username) =>
        await _contractControlService.GetStateChangeOptionsAsync(contractClientId, username);

    public async Task<ActionResponse<ContractClient>> ChangeStateAsync(Guid contractClientId, int newState, string? motivo, string username) =>
        await _contractControlService.ChangeStateAsync(contractClientId, newState, motivo, username);

    public async Task<ActionResponse<IEnumerable<ContractClient>>> GetControlContratos(PaginationDTO pagination, string username) => await _contractControlService.GetControlContratos(pagination, username);

    public async Task<ActionResponse<ContractClient>> GetAsync(Guid id) => await _contractControlService.GetAsync(id);

    public async Task<ActionResponse<ContractClient>> ActivateAsync(Guid id, string username) => await _contractControlService.ActivateAsync(id, username);
}
