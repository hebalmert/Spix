using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class IpNetworkServiceX : IIpNetworkServiceX
{
    private readonly IIpNetworkService _ipNetworkService;

    public IpNetworkServiceX(IIpNetworkService ipNetworkService)
    {
        _ipNetworkService = ipNetworkService;
    }

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string username, Guid? id = null) => await _ipNetworkService.ComboAsync(username, id);

    public async Task<ActionResponse<IpSummaryDto>> GetSummaryAsync(string username) => await _ipNetworkService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string username) => await _ipNetworkService.GetAsync(pagination, username);

    public async Task<ActionResponse<IpNetwork>> GetAsync(Guid id, string username) => await _ipNetworkService.GetAsync(id, username);

    public async Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo, string username) => await _ipNetworkService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string username) => await _ipNetworkService.AddAsync(modelo, username);

    public async Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username) => await _ipNetworkService.AddPoolAsync(modelo, username);

    public async Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username) => await _ipNetworkService.DeletePoolAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _ipNetworkService.DeleteAsync(id, username);
}
