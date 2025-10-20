using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesInven;

public interface ITransferUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus();

    Task<ActionResponse<IEnumerable<Transfer>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Transfer>> GetAsync(Guid id);

    Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo);

    Task<ActionResponse<Transfer>> AddAsync(Transfer modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}