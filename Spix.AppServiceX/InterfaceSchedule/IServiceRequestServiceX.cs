using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IServiceRequestServiceX
{
    Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, int? status, string username);

    Task<ActionResponse<ServiceRequestSummaryDto>> GetSummaryAsync(string username);
    Task<ActionResponse<IEnumerable<ServiceRequestContractDto>>> SearchContractsAsync(string filter, string username);

    Task<ActionResponse<ServiceRequestDto>> GetAsync(Guid id, string username);
    Task<ActionResponse<ServiceRequestDto>> AddAsync(ServiceRequestDto dto, string username);

    Task<ActionResponse<ServiceRequestDto>> UpdateAsync(ServiceRequestDto dto, string username);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);

    Task<ActionResponse<ServiceRequestDto>> AssignAsync(Guid id, Guid technicianId, DateTime scheduledAtUtc, string username);

    Task<ActionResponse<ServiceRequestDto>> ResolveByPhoneAsync(Guid id, string? comment, string? recommendation, string username);

    Task<ActionResponse<ServiceRequestDto>> CloseAsync(Guid id, string? comment, string? recommendation, string username);

}
