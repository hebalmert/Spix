using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEntitiesData;

public interface IFrecuencyService
{
    Task<ActionResponse<IEnumerable<Frecuency>>> ComboAsync(int id);

    Task<ActionResponse<IEnumerable<Frecuency>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Frecuency>> GetAsync(int id);

    Task<ActionResponse<Frecuency>> UpdateAsync(Frecuency modelo);

    Task<ActionResponse<Frecuency>> AddAsync(Frecuency modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}