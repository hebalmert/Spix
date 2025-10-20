using Spix.Core.EntitiesNet;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfaceEntitiesNet;

public interface IIpNetUnitOfWork
{
    Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<IpNet>> GetAsync(Guid id);

    Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo);

    Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}