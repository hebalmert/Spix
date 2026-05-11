using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesMk;

public interface IMkConnectionServiceX
{
    Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Guid serverId, string username);
}
