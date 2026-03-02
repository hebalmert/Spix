using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.InterfacesEntitiesData;

public interface IFrecuencyUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(int id);

    Task<ActionResponse<IEnumerable<Frecuency>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Frecuency>> GetAsync(int id);

    Task<ActionResponse<Frecuency>> UpdateAsync(Frecuency modelo);

    Task<ActionResponse<Frecuency>> AddAsync(Frecuency modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}