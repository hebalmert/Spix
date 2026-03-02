using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfaceEntitiesNet;

public interface IServerUnitOfWork
{
    Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Server>> GetAsync(Guid id);

    Task<ActionResponse<Server>> UpdateAsync(Server modelo);

    Task<ActionResponse<Server>> AddAsync(Server modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}