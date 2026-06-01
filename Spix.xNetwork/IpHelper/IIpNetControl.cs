using Microsoft.EntityFrameworkCore.Storage;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.IpHelper;

public interface IIpNetControl
{
    Task<ActionResponse<IpNet>> SelectIpNetWhenAdd(Guid id, string description, IDbContextTransaction transaction);
    Task<ActionResponse<IpNet>> SelectIpNetWhenUpdate(Guid id, Guid entityId, string description, IDbContextTransaction transaction);
    Task<ActionResponse<IpNet>> SelectIpNetToDelete(Guid id, IDbContextTransaction transaction);
}
