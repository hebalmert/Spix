using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceContratos;

public interface IContractIDPicServiceX
{
    Task<ActionResponse<ContractIDPic>> GetAsync(Guid id);

    Task<ActionResponse<ContractIDPic>> UpdateAsync(ContractIDPic modelo);

    Task<ActionResponse<ContractIDPic>> AddAsync(ContractIDPic modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
