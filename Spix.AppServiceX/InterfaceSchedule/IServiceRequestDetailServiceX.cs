using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IServiceRequestDetailServiceX
{
    Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username);

    Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username);
}
