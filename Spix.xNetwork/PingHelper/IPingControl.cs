using Spix.DomainLogic.ModelUtility;

namespace Spix.xNetwork.PingHelper;

public interface IPingControl
{
    Task<ActionResponse<PingResult>> PingAsync(string host, int attempts = 4, int timeout = 4000);
}
