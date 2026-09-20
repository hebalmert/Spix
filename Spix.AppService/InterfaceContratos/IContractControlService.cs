using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IContractControlService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetStateChangeOptionsAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractClient>> ChangeStateAsync(Guid contractClientId, int newState, string? motivo, string username);

    Task<ActionResponse<IEnumerable<ContractClient>>> GetControlContratos(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractClient>> GetAsync(Guid id);

    Task<ActionResponse<ContractClient>> ActivateAsync(Guid id, string username);
}
