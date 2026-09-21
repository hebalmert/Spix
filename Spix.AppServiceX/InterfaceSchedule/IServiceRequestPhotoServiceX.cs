using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IServiceRequestPhotoServiceX
{
    Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username);

    Task<ActionResponse<bool>> DeletePhotoAsync(Guid id, string username);
}
