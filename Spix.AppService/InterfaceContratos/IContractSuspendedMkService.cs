using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

// Suspender y reactivar DESDE EL ESCRITORIO.
//
// El escritorio configura el equipo por la red LAN, porque el cliente puede no tener IP
// publica. Por eso el trabajo va partido en dos: primero se piden los datos, el escritorio
// escribe el equipo, y despues se guarda.
//
// Vive aparte de ContractSuspendedService para no meterle mano a lo que ya funciona en
// produccion con Blazor, que hace las dos cosas de un solo viaje.
public interface IContractSuspendedMkService
{
    Task<ActionResponse<SuspendMkSetupDTO>> GetSuspendSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<ReactivateMkSetupDTO>> GetReactivateSetupAsync(Guid contractClientId, string username);

    //Guardan lo que el escritorio YA escribio en el equipo: aqui no se toca el MikroTik
    Task<ActionResponse<bool>> SuspendSaveAsync(Guid contractClientId, string? motivo, string username);

    Task<ActionResponse<bool>> ReactivateSaveAsync(Guid contractClientId, string username);
}
