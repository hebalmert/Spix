using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class IpNetServiceX : IIpNetServiceX
{
    private readonly IIpNetService _ipNetService;

    public IpNetServiceX(IIpNetService ipNetService)
    {
        _ipNetService = ipNetService;
    }

    public async Task<ActionResponse<IEnumerable<IpNet>>> ComboAsync(string username, Guid? id = null) => await _ipNetService.ComboAsync(username, id);

    public async Task<ActionResponse<IpSummaryDto>> GetSummaryAsync(string username) => await _ipNetService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string username) => await _ipNetService.GetAsync(pagination, username);

    public async Task<ActionResponse<IpNet>> GetAsync(Guid id, string username) => await _ipNetService.GetAsync(id, username);

    public async Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo, string username) => await _ipNetService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string username) => await _ipNetService.AddAsync(modelo, username);

    public async Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username) => await _ipNetService.AddPoolAsync(modelo, username);

    public async Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username) => await _ipNetService.DeletePoolAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _ipNetService.DeleteAsync(id, username);
}
