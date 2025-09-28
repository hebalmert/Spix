using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface ISecurityService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Security>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Security>> GetAsync(int id);

    Task<ActionResponse<Security>> UpdateAsync(Security modelo);

    Task<ActionResponse<Security>> AddAsync(Security modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}