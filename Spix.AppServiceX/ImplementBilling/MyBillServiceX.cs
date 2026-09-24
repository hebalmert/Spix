using Spix.AppService.InterfacesBilling;
using Spix.AppServiceX.InterfacesBilling;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementBilling;

public class MyBillServiceX : IMyBillServiceX
{
    private readonly IMyBillService _myBillService;

    public MyBillServiceX(IMyBillService myBillService)
    {
        _myBillService = myBillService;
    }

    public async Task<ActionResponse<IEnumerable<MyBillItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        return await _myBillService.GetAsync(pagination, username);
    }

    public async Task<ActionResponse<MyBillDetailDto>> GetAsync(Guid sellId, string username)
    {
        return await _myBillService.GetAsync(sellId, username);
    }

    public async Task<ActionResponse<MyBillSummaryDto>> GetSummaryAsync(string username)
    {
        return await _myBillService.GetSummaryAsync(username);
    }
}
