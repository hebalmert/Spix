using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface IServiceRequestPhotoService
{
    Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username);

    Task<ActionResponse<bool>> DeletePhotoAsync(Guid id, string username);
}
