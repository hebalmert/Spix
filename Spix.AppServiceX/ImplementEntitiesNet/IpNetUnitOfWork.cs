using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntitiesNet;
using Spix.UnitOfWork.InterfaceEntitiesNet;

namespace Spix.UnitOfWork.ImplementEntitiesNet;

public class IpNetUnitOfWork : IIpNetUnitOfWork
{
    private readonly IIpNetService _ipNetService;

    public IpNetUnitOfWork(IIpNetService ipNetService)
    {
        _ipNetService = ipNetService;
    }

    public async Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string email) => await _ipNetService.GetAsync(pagination, email);

    public async Task<ActionResponse<IpNet>> GetAsync(Guid id) => await _ipNetService.GetAsync(id);

    public async Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo) => await _ipNetService.UpdateAsync(modelo);

    public async Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string email) => await _ipNetService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _ipNetService.DeleteAsync(id);
}