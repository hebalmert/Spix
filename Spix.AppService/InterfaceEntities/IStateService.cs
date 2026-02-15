using Spix.Domain.Entities;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntities;

public interface IStateService
{
    Task<ActionResponse<IEnumerable<State>>> ComboAsync(ClaimsDTOs claimsDTO);

    Task<ActionResponse<IEnumerable<State>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<State>> GetAsync(int id);

    Task<ActionResponse<State>> UpdateAsync(State modelo);

    Task<ActionResponse<State>> AddAsync(State modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}