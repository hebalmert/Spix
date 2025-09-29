using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class ServiceClientUnitOfWork : IServiceClientUnitOfWork
{
    private readonly IServiceClientService _serviceClientService;

    public ServiceClientUnitOfWork(IServiceClientService serviceClientService)
    {
        _serviceClientService = serviceClientService;
    }

    public async Task<ActionResponse<IEnumerable<ServiceClient>>> ComboAsync(string username, Guid id) => await _serviceClientService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<ServiceClient>>> GetAsync(PaginationDTO pagination, string username) => await _serviceClientService.GetAsync(pagination, username);

    public async Task<ActionResponse<ServiceClient>> GetAsync(Guid id) => await _serviceClientService.GetAsync(id);

    public async Task<ActionResponse<ServiceClient>> UpdateAsync(ServiceClient modelo) => await _serviceClientService.UpdateAsync(modelo);

    public async Task<ActionResponse<ServiceClient>> AddAsync(ServiceClient modelo, string username) => await _serviceClientService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _serviceClientService.DeleteAsync(id);
}