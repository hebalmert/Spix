using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesBilling;

public interface IMyBillService
{
    Task<ActionResponse<IEnumerable<MyBillItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<MyBillDetailDto>> GetAsync(Guid sellId, string username);

    Task<ActionResponse<MyBillSummaryDto>> GetSummaryAsync(string username);
}
