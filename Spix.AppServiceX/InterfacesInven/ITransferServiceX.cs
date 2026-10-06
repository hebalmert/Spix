using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface ITransferServiceX
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<Transfer>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Transfer>> GetAsync(Guid id, string username);

    Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo, string username);

    Task<ActionResponse<Transfer>> AddAsync(Transfer modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}