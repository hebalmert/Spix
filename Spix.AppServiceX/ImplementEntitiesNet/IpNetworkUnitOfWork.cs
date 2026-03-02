using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntitiesNet;
using Spix.UnitOfWork.InterfaceEntitiesNet;

namespace Spix.UnitOfWork.ImplementEntitiesNet;

public class IpNetworkUnitOfWork : IIpNetworkUnitOfWork
{
    private readonly IIpNetworkService _ipNetworkService;

    public IpNetworkUnitOfWork(IIpNetworkService ipNetworkService)
    {
        _ipNetworkService = ipNetworkService;
    }

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string email, Guid? id = null) => await _ipNetworkService.ComboAsync(email, id);

    public async Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string email) => await _ipNetworkService.GetAsync(pagination, email);

    public async Task<ActionResponse<IpNetwork>> GetAsync(Guid id) => await _ipNetworkService.GetAsync(id);

    public async Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo) => await _ipNetworkService.UpdateAsync(modelo);

    public async Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string email) => await _ipNetworkService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _ipNetworkService.DeleteAsync(id);
}