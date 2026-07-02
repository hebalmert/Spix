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

    public async Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, string username) => await _serviceRequestService.GetAsync(pagination, username);
    public async Task<ActionResponse<IEnumerable<ServiceRequestContractDto>>> SearchContractsAsync(string filter, string username) => await _serviceRequestService.SearchContractsAsync(filter, username);
    public async Task<ActionResponse<ServiceRequestDto>> GetAsync(Guid id, string username) => await _serviceRequestService.GetAsync(id, username);
    public async Task<ActionResponse<ServiceRequestDto>> AddAsync(ServiceRequestDto dto, string username) => await _serviceRequestService.AddAsync(dto, username);
    public async Task<ActionResponse<ServiceRequestDto>> UpdateAsync(ServiceRequestDto dto, string username) => await _serviceRequestService.UpdateAsync(dto, username);
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _serviceRequestService.DeleteAsync(id, username);
    public async Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username) => await _serviceRequestService.AddDetailAsync(dto, username);
    public async Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username) => await _serviceRequestService.DeleteDetailAsync(id, username);
}
