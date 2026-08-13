using Spix.AppWpf.Services.Network.Models;
using Spix.Domain.EntitiesNet;
using Spix.xNetwork.MkHelper;

namespace Spix.AppWpf.Services.Network;

// Ejecuta la API de MikroTik desde LAN o Wi-Fi sin redirigir la conexion por el Backend.
public class LocalMikrotikService : ILocalMikrotikService
{
    private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(10);

    public async Task<LocalMikrotikConnectionResult> CheckConnectionAsync(
        Server server,
        CancellationToken cancellationToken = default)
    {
        var commandResult = await ExecuteAsync(
            server,
            _ => { },
            cancellationToken);

        return new LocalMikrotikConnectionResult
        {
            WasConnected = commandResult.WasExecuted,
            Message = commandResult.Message
        };
    }

    public async Task<LocalMikrotikCommandResult> ExecuteAsync(
        Server server,
        Action<MK> action,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = GetValidationMessage(server);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return new LocalMikrotikCommandResult
            {
                WasExecuted = false,
                Message = validationMessage
            };
        }

        try
        {
            var connectionTask = Task.Run(() => ConnectAndExecute(server, action), CancellationToken.None);
            bool wasConnected = await connectionTask.WaitAsync(ConnectionTimeout, cancellationToken);

            return wasConnected
                ? new LocalMikrotikCommandResult
                {
                    WasExecuted = true,
                    Message = $"Conexion local establecida con {server.ServerName}."
                }
                : new LocalMikrotikCommandResult
                {
                    WasExecuted = false,
                    Message = "MikroTik rechazo las credenciales configuradas para este servidor."
                };
        }
        catch (TimeoutException)
        {
            return new LocalMikrotikCommandResult
            {
                WasExecuted = false,
                Message = "No fue posible conectar localmente con MikroTik dentro del tiempo esperado."
            };
        }
        catch (OperationCanceledException)
        {
            return new LocalMikrotikCommandResult
            {
                WasExecuted = false,
                Message = "La conexion local con MikroTik fue cancelada."
            };
        }
        catch (Exception exception)
        {
            return new LocalMikrotikCommandResult
            {
                WasExecuted = false,
                Message = $"No fue posible conectar localmente con MikroTik: {exception.Message}"
            };
        }
    }

    // Abre, autentica, ejecuta una orden y siempre libera la conexion TCP local.
    private static bool ConnectAndExecute(Server server, Action<MK> action)
    {
        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(
                server.IpNetwork!.Ip!,
                server.ApiPort);

            if (!mikrotik.Login(server.Usuario, server.Clave))
            {
                return false;
            }

            action(mikrotik);
            return true;
        }
        finally
        {
            mikrotik?.Close();
        }
    }

    // Evita intentar la conexion cuando el servidor aun no tiene configuracion completa.
    private static string? GetValidationMessage(Server? server)
    {
        if (server is null)
        {
            return "No fue posible identificar el servidor MikroTik.";
        }

        if (string.IsNullOrWhiteSpace(server.IpNetwork?.Ip))
        {
            return "El servidor no tiene una direccion IP de red configurada.";
        }

        if (server.ApiPort <= 0)
        {
            return "El servidor no tiene un puerto API valido.";
        }

        if (string.IsNullOrWhiteSpace(server.Usuario) || string.IsNullOrWhiteSpace(server.Clave))
        {
            return "El servidor no tiene credenciales MikroTik configuradas.";
        }

        return null;
    }
}
