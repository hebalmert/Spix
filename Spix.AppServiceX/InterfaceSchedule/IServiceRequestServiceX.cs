using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface IServiceRequestServiceX
{
    Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, string username);
    Task<ActionResponse<IEnumerable<ServiceRequestContractDto>>> SearchContractsAsync(string filter, string username);
    Task<ActionResponse<ServiceRequestDto>> GetAsync(Guid id, string username);
    Task<ActionResponse<ServiceRequestDto>> AddAsync(ServiceRequestDto dto, string username);
    Task<ActionResponse<ServiceRequestDto>> UpdateAsync(ServiceRequestDto dto, string username);
    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
    Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username);
    Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username);
}
