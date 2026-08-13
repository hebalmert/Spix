using Spix.DomainLogic.ModelUtility;

namespace Spix.AppWpf.NetHelper;

// Define pings que se ejecutan desde la conexion local del escritorio WPF.
public interface IPingControl
{
    Task<ActionResponse<PingResult>> PingAsync(
        string host,
        int attempts = 4,
        int timeout = 4000);
}
