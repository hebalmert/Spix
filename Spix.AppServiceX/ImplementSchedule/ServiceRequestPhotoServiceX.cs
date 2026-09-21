using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class ServiceRequestPhotoServiceX : IServiceRequestPhotoServiceX
{
    private readonly IServiceRequestPhotoService _serviceRequestPhotoService;

    public ServiceRequestPhotoServiceX(IServiceRequestPhotoService serviceRequestPhotoService)
    {
        _serviceRequestPhotoService = serviceRequestPhotoService;
    }

    public async Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username) =>
        await _serviceRequestPhotoService.AddPhotoAsync(dto, username);

    public async Task<ActionResponse<bool>> DeletePhotoAsync(Guid id, string username) =>
        await _serviceRequestPhotoService.DeletePhotoAsync(id, username);
}
