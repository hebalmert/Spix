using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfaceEntities;

public interface ICountryServices
{
    Task<ActionResponse<IEnumerable<Country>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Country>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Country>> GetAsync(int id);

    Task<ActionResponse<Country>> UpdateAsync(Country modelo);

    Task<ActionResponse<Country>> AddAsync(Country modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}