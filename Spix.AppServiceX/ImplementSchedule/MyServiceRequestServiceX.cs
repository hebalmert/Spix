using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class MyServiceRequestServiceX : IMyServiceRequestServiceX
{
    private readonly IMyServiceRequestService _myServiceRequestService;

    public MyServiceRequestServiceX(IMyServiceRequestService myServiceRequestService)
    {
        _myServiceRequestService = myServiceRequestService;
    }

    public async Task<ActionResponse<IEnumerable<MyServiceRequestItemDto>>> GetAsync(string username) =>
        await _myServiceRequestService.GetAsync(username);

    public async Task<ActionResponse<IEnumerable<MyContractItemDto>>> GetMyContractsAsync(string username) =>
        await _myServiceRequestService.GetMyContractsAsync(username);

    public async Task<ActionResponse<bool>> AddAsync(MyServiceRequestDto dto, string username) =>
        await _myServiceRequestService.AddAsync(dto, username);
}
