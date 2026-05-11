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
}
