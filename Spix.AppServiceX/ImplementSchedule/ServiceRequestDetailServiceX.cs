using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class ServiceRequestDetailServiceX : IServiceRequestDetailServiceX
{
    private readonly IServiceRequestDetailService _serviceRequestDetailService;

    public ServiceRequestDetailServiceX(IServiceRequestDetailService serviceRequestDetailService)
    {
        _serviceRequestDetailService = serviceRequestDetailService;
    }

    public async Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username) =>
        await _serviceRequestDetailService.AddDetailAsync(dto, username);

    public async Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username) =>
        await _serviceRequestDetailService.DeleteDetailAsync(id, username);
}
