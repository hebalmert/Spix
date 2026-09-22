using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
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

    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username) => await _serverService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<ServerListItemDto>>> GetAsync(PaginationDTO pagination, string username) => await _serverService.GetAsync(pagination, username);

    public async Task<ActionResponse<Server>> GetAsync(Guid id, string username, bool withCredentials) => await _serverService.GetAsync(id, username, withCredentials);

    public async Task<ActionResponse<Server>> UpdateAsync(Server modelo, string username) => await _serverService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Server>> AddAsync(Server modelo, string username) => await _serverService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _serverService.DeleteAsync(id, username);
}
