using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfaceEntities;

public interface ICorporationUnitOfWork
{
    Task<ActionResponse<IEnumerable<Corporation>>> ComboAsync();

    Task<ActionResponse<IEnumerable<Corporation>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<Corporation>> GetAsync(int id);

    Task<ActionResponse<Corporation>> UpdateAsync(Corporation modelo);

    Task<ActionResponse<Corporation>> AddAsync(Corporation modelo);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}