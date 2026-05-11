using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfacesMk;

public interface IMkConnectionService
{
    Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Guid serverId, string username);
}
