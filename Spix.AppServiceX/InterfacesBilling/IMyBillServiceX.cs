using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesBilling;

public interface IMyBillServiceX
{
    Task<ActionResponse<IEnumerable<MyBillItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<MyBillDetailDto>> GetAsync(Guid sellId, string username);

    Task<ActionResponse<MyBillSummaryDto>> GetSummaryAsync(string username);
}
