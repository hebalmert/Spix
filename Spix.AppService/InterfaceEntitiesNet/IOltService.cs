using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntitiesNet;

public interface IOltService
{
    Task<ActionResponse<IEnumerable<Olt>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<OltListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Olt>> GetAsync(Guid id, string username, bool withCredentials);

    Task<ActionResponse<Olt>> UpdateAsync(Olt modelo, string username);

    Task<ActionResponse<Olt>> AddAsync(Olt modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
