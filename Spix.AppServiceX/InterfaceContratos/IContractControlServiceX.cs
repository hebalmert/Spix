using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractControlServiceX
{
    Task<ActionResponse<IEnumerable<ContractClient>>> GetControlContratos(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractClient>> GetAsync(Guid id);
}
