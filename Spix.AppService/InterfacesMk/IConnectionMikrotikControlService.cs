using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesMk;

public interface IConnectionMikrotikControlService
{
    Task<ActionResponse<IEnumerable<ConnectionMikrotikControl>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ConnectionMikrotikControl>> GetAsync(Guid id);

    Task<ActionResponse<ConnectionMikrotikControl>> UpdateAsync(ConnectionMikrotikControl modelo);

    Task<ActionResponse<ConnectionMikrotikControl>> AddAsync(ConnectionMikrotikControl modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
