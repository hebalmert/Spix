using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesGen;

public interface IServiceClientService
{
    Task<ActionResponse<IEnumerable<ServiceClient>>> ComboAsync(string username, Guid id);

    Task<ActionResponse<IEnumerable<ServiceClient>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ServiceClient>> GetAsync(Guid id);

    Task<ActionResponse<ServiceClient>> UpdateAsync(ServiceClient modelo);

    Task<ActionResponse<ServiceClient>> AddAsync(ServiceClient modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}