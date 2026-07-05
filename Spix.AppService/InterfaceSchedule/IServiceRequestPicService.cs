using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface IServiceRequestPicService
{
    Task<ActionResponse<ServiceRequestPic>> GetByServiceRequestAsync(Guid serviceRequestId, string username);
    Task<ActionResponse<ServiceRequestPic>> GetAsync(Guid id, string username);
    Task<ActionResponse<ServiceRequestPic>> UpdateAsync(ServiceRequestPic modelo, string username);
    Task<ActionResponse<ServiceRequestPic>> AddAsync(ServiceRequestPic modelo, string username);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
