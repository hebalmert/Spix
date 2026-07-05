using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class ServiceRequestPicServiceX : IServiceRequestPicServiceX
{
    private readonly IServiceRequestPicService _serviceRequestPicService;

    public ServiceRequestPicServiceX(IServiceRequestPicService serviceRequestPicService)
    {
        _serviceRequestPicService = serviceRequestPicService;
    }

    public async Task<ActionResponse<ServiceRequestPic>> GetByServiceRequestAsync(Guid serviceRequestId, string username) => await _serviceRequestPicService.GetByServiceRequestAsync(serviceRequestId, username);
    public async Task<ActionResponse<ServiceRequestPic>> GetAsync(Guid id, string username) => await _serviceRequestPicService.GetAsync(id, username);
    public async Task<ActionResponse<ServiceRequestPic>> UpdateAsync(ServiceRequestPic modelo, string username) => await _serviceRequestPicService.UpdateAsync(modelo, username);
    public async Task<ActionResponse<ServiceRequestPic>> AddAsync(ServiceRequestPic modelo, string username) => await _serviceRequestPicService.AddAsync(modelo, username);
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _serviceRequestPicService.DeleteAsync(id, username);
}
