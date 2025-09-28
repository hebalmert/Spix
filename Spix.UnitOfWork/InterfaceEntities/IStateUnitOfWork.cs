using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ResponcesSec;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfaceEntities;

public interface IStateUnitOfWork
{
    Task<ActionResponse<IEnumerable<State>>> ComboAsync(ClaimsDTOs claimsDTO);

    Task<ActionResponse<IEnumerable<State>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<State>> GetAsync(int id);

    Task<ActionResponse<State>> UpdateAsync(State modelo);

    Task<ActionResponse<State>> AddAsync(State modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}