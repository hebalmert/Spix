namespace Spix.AppWpf.Services.Network.Models;

// Comunica si una orden local llego a ejecutarse contra el equipo MikroTik.
public class LocalMikrotikCommandResult
{
    public bool WasExecuted { get; init; }

    public string Message { get; init; } = string.Empty;
}
