using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementSchedule;

public class VisitReviewServiceX : IVisitReviewServiceX
{
    private readonly IVisitReviewService _visitReviewService;

    public VisitReviewServiceX(IVisitReviewService visitReviewService)
    {
        _visitReviewService = visitReviewService;
    }

    public async Task<ActionResponse<VisitReviewCountersDto>> GetCountersAsync(string username) =>
        await _visitReviewService.GetCountersAsync(username);

    public async Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetLocationAsync(PaginationDTO pagination, string username) =>
        await _visitReviewService.GetLocationAsync(pagination, username);

    public async Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetAbsentAsync(PaginationDTO pagination, string username) =>
        await _visitReviewService.GetAbsentAsync(pagination, username);

    public async Task<ActionResponse<bool>> ApplyLocationAsync(Guid serviceRequestId, string username) =>
        await _visitReviewService.ApplyLocationAsync(serviceRequestId, username);

    public async Task<ActionResponse<bool>> DismissAsync(Guid serviceRequestId, string username) =>
        await _visitReviewService.DismissAsync(serviceRequestId, username);
}
