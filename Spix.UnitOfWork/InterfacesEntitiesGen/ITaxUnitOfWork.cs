using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface ITaxUnitOfWork
{
    Task<ActionResponse<IEnumerable<Tax>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Tax>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Tax>> GetAsync(Guid id);

    Task<ActionResponse<Tax>> UpdateAsync(Tax modelo);

    Task<ActionResponse<Tax>> AddAsync(Tax modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}