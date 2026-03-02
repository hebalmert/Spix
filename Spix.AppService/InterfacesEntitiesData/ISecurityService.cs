using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesData;

public interface ISecurityService
{
    Task<ActionResponse<IEnumerable<Security>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Security>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Security>> GetAsync(int id);

    Task<ActionResponse<Security>> UpdateAsync(Security modelo);

    Task<ActionResponse<Security>> AddAsync(Security modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}