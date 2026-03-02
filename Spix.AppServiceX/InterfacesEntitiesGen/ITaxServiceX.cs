using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesEntitiesGen;

public interface ITaxServiceX
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Tax>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Tax>> GetAsync(Guid id);

    Task<ActionResponse<Tax>> UpdateAsync(Tax modelo);

    Task<ActionResponse<Tax>> AddAsync(Tax modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}