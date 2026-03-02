using Microsoft.EntityFrameworkCore.Storage;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.IpHelper;

public interface IIpControl
{
    Task<ActionResponse<IpNetwork>> SelectIpWhenAdd(Guid id, string description, IDbContextTransaction transaction);
    Task<ActionResponse<IpNetwork>> SelectIpWhenUpdate(Guid id, Guid entityId, string description, IDbContextTransaction transaction);
    Task<ActionResponse<IpNetwork>> SelectIpWhenUpdateServer(Guid id, Guid serverId, string description, IDbContextTransaction transaction);
    Task<ActionResponse<IpNetwork>> SelectIpToDelete(Guid id, IDbContextTransaction transaction);
}
