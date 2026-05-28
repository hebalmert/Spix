using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceContratos;

public interface IContractClientService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatusAsync();

    Task<ActionResponse<IEnumerable<ContractClient>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractClient>> GetAsync(Guid id);

    Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo);

    Task<ActionResponse<ContractClient>> AddAsync(ContractClient modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}