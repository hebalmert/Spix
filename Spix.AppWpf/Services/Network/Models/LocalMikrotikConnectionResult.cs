namespace Spix.AppWpf.Services.Network.Models;

// Describe el resultado de una conexion local a un servidor MikroTik.
public class LocalMikrotikConnectionResult
{
    public bool WasConnected { get; init; }

    public string Message { get; init; } = string.Empty;
}
