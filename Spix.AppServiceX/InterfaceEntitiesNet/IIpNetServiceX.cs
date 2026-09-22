using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfaceEntitiesNet;

public interface IIpNetServiceX
{
    Task<ActionResponse<IEnumerable<IpNet>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<IpSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<IpNet>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IpNet>> GetAsync(Guid id, string username);

    Task<ActionResponse<IpNet>> UpdateAsync(IpNet modelo, string username);

    Task<ActionResponse<IpNet>> AddAsync(IpNet modelo, string username);

    Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username);

    Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
