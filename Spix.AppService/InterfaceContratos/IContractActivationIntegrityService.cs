using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

public interface IContractActivationIntegrityService
{
    //=========================================================================
    //Los metodos de ENTRADA. El que llama no pregunta de que tipo es el equipo:
    //solo llama, y aca adentro se resuelve por el servidor del contrato.
    //=========================================================================

    //Como trabaja el equipo de este contrato, o de este servidor
    Task<MikrotikControlType> ResolveControlAsync(Guid contractClientId);

    Task<MikrotikControlType> ResolveControlByServerAsync(Guid serverId);

    //Le falta algo al contrato para poder tocarle el acceso?
    Task<ActionResponse<bool>> ValidateAsync(Guid contractClientId);

    //Que le impide pasar a ese estado. Null si nada.
    Task<string?> GetBlockingReasonAsync(Guid contractClientId, ContractState target);

    //El servicio esta prendido en el equipo?
    Task<bool> IsServiceOnAsync(Guid contractClientId);

    //Devolverle el acceso
    Task<ActionResponse<bool>> ActivateAsync(ContractClient contract);

    Task<ActionResponse<bool>> ActivateAsync(IEnumerable<ContractClient> contracts);

    //Quitarle el acceso
    Task<ActionResponse<bool>> SuspendAsync(ContractClient contract);

    Task<ActionResponse<bool>> SuspendAsync(IEnumerable<ContractClient> contracts);

    //Se puede hablar con los equipos de estos contratos?
    Task<ActionResponse<bool>> VerifyConnectionAsync(ContractClient contract);

    Task<ActionResponse<bool>> VerifyConnectionAsync(IEnumerable<Guid> contractClientIds);

    //Las piezas que le faltan al contrato, para pintarlas en la pantalla
    Task<List<string>> MissingItemsAsync(Guid contractClientId);

    //=========================================================================
    //Lo especifico de HotSpot. Lo siguen usando los servicios que preparan los
    //datos del escritorio, que hablan de bindings por su nombre.
    //=========================================================================

    Task<ActionResponse<bool>> ActivateHotSpotBindingsAsync(ContractClient contract);

    Task<ActionResponse<bool>> VerifyHotSpotBindingsConnectionAsync(ContractClient contract);

    Task<ActionResponse<bool>> VerifyHotSpotServersConnectionAsync(IEnumerable<Guid> contractClientIds);

    Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(ContractClient contract);

    Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(IEnumerable<ContractClient> contracts);
}
