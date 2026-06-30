using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceEntitiesNet;

public interface IServerServiceX
{
    Task<ActionResponse<IEnumerable<Server>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Server>> GetAsync(Guid id);

    Task<ActionResponse<Server>> UpdateAsync(Server modelo);

    Task<ActionResponse<Server>> AddAsync(Server modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
