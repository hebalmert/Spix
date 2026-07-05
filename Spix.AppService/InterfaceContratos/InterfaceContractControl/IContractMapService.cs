using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractMapService
{
    Task<ActionResponse<ContractMap>> GetAsync(Guid id);

    Task<ActionResponse<ContractMap>> AddAsync(ContractMap modelo, string username);

    Task<ActionResponse<ContractMap>> UpdateAsync(ContractMap modelo);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
