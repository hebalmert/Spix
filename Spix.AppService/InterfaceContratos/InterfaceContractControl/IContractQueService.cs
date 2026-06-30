using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

public interface IContractQueService
{
    Task<ActionResponse<ContractQue>> GetAsync(Guid id);

    Task<ActionResponse<ContractQue>> AddAsync(ContractQue modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
