using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractActivationIntegrityService
{
    Task<ActionResponse<bool>> ValidateAsync(Guid contractClientId, int corporationId);

    Task<bool> UsesHotSpotControlAsync(int corporationId);

    Task<ActionResponse<bool>> ActivateHotSpotBindingsAsync(ContractClient contract);

    Task<ActionResponse<bool>> VerifyHotSpotBindingsConnectionAsync(ContractClient contract);

    Task<ActionResponse<bool>> VerifyHotSpotServersConnectionAsync(IEnumerable<Guid> contractClientIds);

    Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(ContractClient contract);

    Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(IEnumerable<ContractClient> contracts);
}
