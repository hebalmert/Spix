using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntitiesNet;
using Spix.UnitOfWork.InterfaceEntitiesNet;

namespace Spix.UnitOfWork.ImplementEntitiesNet;

public class ServerUnitOfWork : IServerUnitOfWork
{
    private readonly IServerService _serverService;

    public ServerUnitOfWork(IServerService serverService)
    {
        _serverService = serverService;
    }

    public async Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string email) => await _serverService.GetAsync(pagination, email);

    public async Task<ActionResponse<Server>> GetAsync(Guid id) => await _serverService.GetAsync(id);

    public async Task<ActionResponse<Server>> UpdateAsync(Server modelo) => await _serverService.UpdateAsync(modelo);

    public async Task<ActionResponse<Server>> AddAsync(Server modelo, string email) => await _serverService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _serverService.DeleteAsync(id);
}