using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractOltService
{
    Task<ActionResponse<ContractOlt>> GetAsync(Guid id, string username);

    Task<ActionResponse<ContractOlt>> AddAsync(ContractOlt modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}
