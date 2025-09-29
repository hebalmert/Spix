using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesEntitiesGen;

public interface IRegisterUnitOfWork
{
    Task<ActionResponse<IEnumerable<Register>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Register>> GetAsync(Guid id);

    Task<ActionResponse<Register>> UpdateAsync(Register modelo);

    Task<ActionResponse<Register>> AddAsync(Register modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}