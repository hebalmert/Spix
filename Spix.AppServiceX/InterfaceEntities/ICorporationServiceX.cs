using Spix.Domain.Entities;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceEntities;

public interface ICorporationServiceX
{
    Task<ActionResponse<IEnumerable<Corporation>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Corporation>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Corporation>> GetAsync(int id);

    Task<ActionResponse<Corporation>> UpdateAsync(Corporation modelo);

    Task<ActionResponse<Corporation>> AddAsync(Corporation modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}