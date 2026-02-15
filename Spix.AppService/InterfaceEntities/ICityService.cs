using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntities;

public interface ICityService
{
    Task<ActionResponse<IEnumerable<City>>> ComboAsync(int id);

    Task<ActionResponse<IEnumerable<City>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<City>> GetAsync(int id);

    Task<ActionResponse<City>> UpdateAsync(City modelo);

    Task<ActionResponse<City>> AddAsync(City modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}