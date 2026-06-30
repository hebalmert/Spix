using Spix.AppService.InterfacesMk;
using Spix.AppServiceX.InterfacesMk;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementMk;

public class ConnectionMikrotikControlServiceX : IConnectionMikrotikControlServiceX
{
    private readonly IConnectionMikrotikControlService _connectionMikrotikControlService;

    public ConnectionMikrotikControlServiceX(IConnectionMikrotikControlService connectionMikrotikControlService)
    {
        _connectionMikrotikControlService = connectionMikrotikControlService;
    }

    public async Task<ActionResponse<IEnumerable<ConnectionMikrotikControl>>> GetAsync(PaginationDTO pagination, string username) => await _connectionMikrotikControlService.GetAsync(pagination, username);

    public async Task<ActionResponse<ConnectionMikrotikControl>> GetAsync(Guid id) => await _connectionMikrotikControlService.GetAsync(id);

    public async Task<ActionResponse<ConnectionMikrotikControl>> UpdateAsync(ConnectionMikrotikControl modelo) => await _connectionMikrotikControlService.UpdateAsync(modelo);

    public async Task<ActionResponse<ConnectionMikrotikControl>> AddAsync(ConnectionMikrotikControl modelo, string username) => await _connectionMikrotikControlService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _connectionMikrotikControlService.DeleteAsync(id);
}
