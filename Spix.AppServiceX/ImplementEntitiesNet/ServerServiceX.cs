using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class ServerServiceX : IServerServiceX
{
    private readonly IServerService _serverService;

    public ServerServiceX(IServerService serverService)
    {
        _serverService = serverService;
    }

    public async Task<ActionResponse<IEnumerable<Server>>> ComboAsync(string username, Guid? id = null) => await _serverService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<Server>>> GetAsync(PaginationDTO pagination, string email) => await _serverService.GetAsync(pagination, email);

    public async Task<ActionResponse<Server>> GetAsync(Guid id) => await _serverService.GetAsync(id);

    public async Task<ActionResponse<Server>> UpdateAsync(Server modelo) => await _serverService.UpdateAsync(modelo);

    public async Task<ActionResponse<Server>> AddAsync(Server modelo, string email) => await _serverService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _serverService.DeleteAsync(id);
}
