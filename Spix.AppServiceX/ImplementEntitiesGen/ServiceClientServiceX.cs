using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class ServiceClientServiceX : IServiceClientServiceX
{
    private readonly IServiceClientService _serviceClientService;

    public ServiceClientServiceX(IServiceClientService serviceClientService)
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