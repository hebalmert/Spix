using Microsoft.EntityFrameworkCore.Storage;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.MacHelper;

public interface IMacControl
{
    Task<ActionResponse<CargueDetail>> SelectMacWhenAdd(Guid id, string description, IDbContextTransaction transaction);
    Task<ActionResponse<CargueDetail>> SelectMacToDelete(Guid id, IDbContextTransaction transaction);
}
