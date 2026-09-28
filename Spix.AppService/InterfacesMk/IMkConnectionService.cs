using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesMk;

public interface IMkConnectionService
{
    Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Guid serverId, string username);

    //Las interfaces reales del equipo, para elegir la de clientes sin escribirla a mano
    Task<ActionResponse<IEnumerable<MkInterfaceDTO>>> InterfacesComboAsync(Guid serverId, string username);

    //Deja el equipo listo para PPPoE: crea el perfil y el servidor PPPoE sobre esa interfaz
    Task<ActionResponse<bool>> CreatePppoeServerAsync(Guid serverId, string? serviceName, string username);

    //Deshace lo anterior, si no hay contratos pegados al servidor
    Task<ActionResponse<bool>> DeletePppoeServerAsync(Guid serverId, string username);

    Task<ActionResponse<PppoeServerLocalSetupDTO>> GetPppoeLocalSetupAsync(Guid serverId, string? serviceName, string username);

    Task<ActionResponse<bool>> SavePppoeLocalAsync(PppoeServerLocalSaveDTO datos, string username);

    //El borrado local: preparar (con la guarda de contratos) y limpiar el espejo
    Task<ActionResponse<PppoeServerLocalRemoveDTO>> GetPppoeLocalRemoveAsync(Guid serverId, string username);

    Task<ActionResponse<bool>> ClearPppoeLocalAsync(Guid serverId, string username);
}
