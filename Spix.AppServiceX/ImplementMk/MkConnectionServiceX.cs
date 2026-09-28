using Spix.AppService.InterfacesMk;
using Spix.AppServiceX.InterfacesMk;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementMk;

public class MkConnectionServiceX : IMkConnectionServiceX
{
    private readonly IMkConnectionService _mkConnection;

    public MkConnectionServiceX(IMkConnectionService mkConnection)
    {
        _mkConnection = mkConnection;
    }

    public async Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Guid serverId, string username) => await _mkConnection.CheckConnectionAsync(serverId, username);

    public async Task<ActionResponse<IEnumerable<MkInterfaceDTO>>> InterfacesComboAsync(Guid serverId, string username) => await _mkConnection.InterfacesComboAsync(serverId, username);

    public async Task<ActionResponse<bool>> CreatePppoeServerAsync(Guid serverId, string? serviceName, string username) => await _mkConnection.CreatePppoeServerAsync(serverId, serviceName, username);

    public async Task<ActionResponse<bool>> DeletePppoeServerAsync(Guid serverId, string username) => await _mkConnection.DeletePppoeServerAsync(serverId, username);

    public async Task<ActionResponse<PppoeServerLocalSetupDTO>> GetPppoeLocalSetupAsync(Guid serverId, string? serviceName, string username)
        => await _mkConnection.GetPppoeLocalSetupAsync(serverId, serviceName, username);

    public async Task<ActionResponse<bool>> SavePppoeLocalAsync(PppoeServerLocalSaveDTO datos, string username)
        => await _mkConnection.SavePppoeLocalAsync(datos, username);

    public async Task<ActionResponse<PppoeServerLocalRemoveDTO>> GetPppoeLocalRemoveAsync(Guid serverId, string username)
        => await _mkConnection.GetPppoeLocalRemoveAsync(serverId, username);

    public async Task<ActionResponse<bool>> ClearPppoeLocalAsync(Guid serverId, string username)
        => await _mkConnection.ClearPppoeLocalAsync(serverId, username);
}
