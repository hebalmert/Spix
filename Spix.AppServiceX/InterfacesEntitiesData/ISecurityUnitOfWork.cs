using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface ISecurityUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Security>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Security>> GetAsync(int id);

    Task<ActionResponse<Security>> UpdateAsync(Security modelo);

    Task<ActionResponse<Security>> AddAsync(Security modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}