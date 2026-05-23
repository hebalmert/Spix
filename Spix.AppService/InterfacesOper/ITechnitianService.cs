using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesOper;

public interface ITechnitianService
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Technician>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Technician>> GetAsync(Guid id);

    Task<ActionResponse<Technician>> UpdateAsync(Technician modelo, string frontUrl);

    Task<ActionResponse<Technician>> AddAsync(Technician modelo, string username, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}