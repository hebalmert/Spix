using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;

public interface IContractMapServiceX
{
    Task<ActionResponse<ContractMap>> GetAsync(Guid id);

    Task<ActionResponse<ContractMap>> AddAsync(ContractMap modelo, string username);

    Task<ActionResponse<ContractMap>> UpdateAsync(ContractMap modelo);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
