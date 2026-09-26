using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos;

// El corte masivo DESDE EL ESCRITORIO, equipo por equipo.
//
// El escritorio le quita el acceso al cliente por la red LAN, porque el cliente puede no
// tener IP publica. Por eso el trabajo va partido: primero se pide el lote del equipo, el
// escritorio escribe, y despues se guarda lo que el equipo acepto.
//
// Vive aparte de RunSuspendedService para no meterle mano a lo que ya funciona con Blazor.
public interface IRunSuspendedMkService
{
    Task<ActionResponse<CorteMkSetupDTO>> GetRunSetupAsync(Guid id, Guid serverId, string username);

    //Guarda lo que el escritorio YA escribio en el equipo: aqui no se toca el MikroTik
    Task<ActionResponse<CorteRunResultDto>> RunSaveAsync(Guid id, Guid serverId, List<Guid> suspendidos, string username);
}
