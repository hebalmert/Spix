using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

// Reactivacion masiva DESDE EL ESCRITORIO, equipo por equipo.
//
// El escritorio le devuelve el acceso al cliente por la red LAN, porque el cliente puede
// no tener IP publica. Por eso el trabajo va partido en dos: primero se piden los datos
// del lote, el escritorio escribe el equipo, y despues se guarda lo que el equipo acepto.
//
// Vive aparte de ActivationService para no meterle mano a lo que ya funciona en produccion
// con Blazor, que hace las dos cosas de un solo viaje.
public interface IActivationMkService
{
    Task<ActionResponse<ActivationMkSetupDTO>> GetActivateSetupAsync(Guid serverId, string username);

    //Guarda lo que el escritorio YA escribio en el equipo: aqui no se toca el MikroTik
    Task<ActionResponse<ActivationRunResultDto>> ActivateSaveAsync(Guid serverId, List<Guid> activados, string username);
}
