using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntitiesNet;

public interface IIpNetService
{
    Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<IpNet>> GetAsync(Guid id);

    Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo);

    Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}