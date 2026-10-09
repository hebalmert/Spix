using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceSchedule;

public interface IVisitReviewService
{
    Task<ActionResponse<VisitReviewCountersDto>> GetCountersAsync(string username);

    Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetLocationAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetAbsentAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<bool>> ApplyLocationAsync(Guid serviceRequestId, string username);

    Task<ActionResponse<bool>> DismissAsync(Guid serviceRequestId, string username);
}
