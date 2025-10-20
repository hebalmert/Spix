using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfaceEntitiesNet;

public interface IServerService
{
    Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Server>> GetAsync(Guid id);

    Task<ActionResponse<Server>> UpdateAsync(Server modelo);

    Task<ActionResponse<Server>> AddAsync(Server modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}