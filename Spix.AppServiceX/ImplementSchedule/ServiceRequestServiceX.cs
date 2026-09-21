using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementSchedule;

public class ServiceRequestServiceX : IServiceRequestServiceX
{
    private readonly IServiceRequestService _serviceRequestService;

    public ServiceRequestServiceX(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    public async Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, int? status, string username) => await _serviceRequestService.GetAsync(pagination, status, username);
    public async Task<ActionResponse<IEnumerable<ServiceRequestContractDto>>> SearchContractsAsync(string filter, string username) => await _serviceRequestService.SearchContractsAsync(filter, username);

    public async Task<ActionResponse<ServiceRequestSummaryDto>> GetSummaryAsync(string username) =>
        await _serviceRequestService.GetSummaryAsync(username);
    public async Task<ActionResponse<ServiceRequestDto>> GetAsync(Guid id, string username) => await _serviceRequestService.GetAsync(id, username);
    public async Task<ActionResponse<ServiceRequestDto>> AddAsync(ServiceRequestDto dto, string username) => await _serviceRequestService.AddAsync(dto, username);

    public async Task<ActionResponse<ServiceRequestDto>> UpdateAsync(ServiceRequestDto dto, string username) => await _serviceRequestService.UpdateAsync(dto, username);
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _serviceRequestService.DeleteAsync(id, username);

    public async Task<ActionResponse<ServiceRequestDto>> AssignAsync(Guid id, Guid technicianId, DateTime scheduledAtUtc, string username) =>
        await _serviceRequestService.AssignAsync(id, technicianId, scheduledAtUtc, username);

    public async Task<ActionResponse<ServiceRequestDto>> ResolveByPhoneAsync(Guid id, string? comment, string? recommendation, string username) =>
        await _serviceRequestService.ResolveByPhoneAsync(id, comment, recommendation, username);

    public async Task<ActionResponse<ServiceRequestDto>> CloseAsync(Guid id, string? comment, string? recommendation, string username) =>
        await _serviceRequestService.CloseAsync(id, comment, recommendation, username);

}
