using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesGen;

public interface IZoneService
{
    Task<ActionResponse<IEnumerable<Zone>>> ComboAsync(string username, int id);

    Task<ActionResponse<IEnumerable<Zone>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Zone>> GetAsync(Guid id);

    Task<ActionResponse<Zone>> UpdateAsync(Zone modelo);

    Task<ActionResponse<Zone>> AddAsync(Zone modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}