using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesGen;

public interface IServiceCategoryService
{
    Task<ActionResponse<IEnumerable<ServiceCategory>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<ServiceCategory>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ServiceCategory>> GetAsync(Guid id);

    Task<ActionResponse<ServiceCategory>> UpdateAsync(ServiceCategory modelo);

    Task<ActionResponse<ServiceCategory>> AddAsync(ServiceCategory modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}