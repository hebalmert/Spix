using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface IServiceRequestDetailService
{
    Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username);

    Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username);
}
