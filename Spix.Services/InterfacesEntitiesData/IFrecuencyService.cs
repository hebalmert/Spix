using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesEntitiesData;

public interface IFrecuencyService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(int id);

    Task<ActionResponse<IEnumerable<Frecuency>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Frecuency>> GetAsync(int id);

    Task<ActionResponse<Frecuency>> UpdateAsync(Frecuency modelo);

    Task<ActionResponse<Frecuency>> AddAsync(Frecuency modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}