using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesMk;

public interface IQueueTypeService
{
    Task<ActionResponse<IEnumerable<QueueType>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<QueueType>> GetAsync(Guid id);

    Task<ActionResponse<QueueType>> UpdateAsync(QueueType modelo);

    Task<ActionResponse<QueueType>> AddAsync(QueueType modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
