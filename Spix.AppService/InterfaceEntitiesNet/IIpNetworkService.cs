using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfaceEntitiesNet;

public interface IIpNetworkService
{
    Task<ActionResponse<IEnumerable<IpNetwork>>> ComboAsync(string username, Guid? id = null);

    Task<ActionResponse<IpSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<IpNetwork>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IpNetwork>> GetAsync(Guid id, string username);

    Task<ActionResponse<IpNetwork>> UpdateAsync(IpNetwork modelo, string username);

    Task<ActionResponse<IpNetwork>> AddAsync(IpNetwork modelo, string username);

    Task<ActionResponse<int>> AddPoolAsync(IpNetPoolCreateDTO modelo, string username);

    Task<ActionResponse<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
