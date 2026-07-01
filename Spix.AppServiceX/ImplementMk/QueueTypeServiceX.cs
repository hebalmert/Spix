using Spix.AppService.InterfacesMk;
using Spix.AppServiceX.InterfacesMk;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementMk;

public class QueueTypeServiceX : IQueueTypeServiceX
{
    private readonly IQueueTypeService _queueTypeService;

    public QueueTypeServiceX(IQueueTypeService queueTypeService)
    {
        _queueTypeService = queueTypeService;
    }

    public async Task<ActionResponse<IEnumerable<QueueType>>> GetAsync(PaginationDTO pagination, string username) => await _queueTypeService.GetAsync(pagination, username);

    public async Task<ActionResponse<QueueType>> GetAsync(Guid id) => await _queueTypeService.GetAsync(id);

    public async Task<ActionResponse<QueueType>> UpdateAsync(QueueType modelo) => await _queueTypeService.UpdateAsync(modelo);

    public async Task<ActionResponse<QueueType>> AddAsync(QueueType modelo, string username) => await _queueTypeService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _queueTypeService.DeleteAsync(id);
}
