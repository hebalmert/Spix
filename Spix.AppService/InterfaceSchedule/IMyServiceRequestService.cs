using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceSchedule;

public interface IMyServiceRequestService
{
    Task<ActionResponse<IEnumerable<MyServiceRequestItemDto>>> GetAsync(string username);

    Task<ActionResponse<IEnumerable<MyContractItemDto>>> GetMyContractsAsync(string username);

    Task<ActionResponse<bool>> AddAsync(MyServiceRequestDto dto, string username);
}
