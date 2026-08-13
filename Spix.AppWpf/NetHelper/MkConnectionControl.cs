using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppWpf.NetHelper;

// Comprueba localmente la identidad y los IP bindings del MikroTik configurado en un servidor.
public class MkConnectionControl : IMkConnectionControl
{
    public async Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Server server)
    {
        string? validationMessage = GetValidationMessage(server);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = false,
                Message = validationMessage
            };
        }

        try
        {
            return await Task.Run(() => CheckConnection(server));
        }
        catch (Exception exception)
        {
            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
    }

    // Ejecuta el protocolo TCP fuera del hilo visual para mantener WPF responsivo.
    private static ActionResponse<MkConnectionResultDTO> CheckConnection(Server server)
    {
        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

            if (!mikrotik.Login(server.Usuario, server.Clave))
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = "No fue posible autenticar las credenciales en MikroTik."
                };
            }

            string mikrotikName = GetMikrotikName(mikrotik);
            int bindings = GetIpBindingsCount(mikrotik);

            var dto = new MkConnectionResultDTO
            {
                Text = $"Conexion exitosa a Mikrotik {mikrotikName}",
                Value = bindings,
                MikrotikName = mikrotikName
            };

            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = true,
                Result = dto
            };
        }
        catch (Exception exception)
        {
            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = false,
                Message = exception.Message
            };
        }
        finally
        {
            mikrotik?.Close();
        }
    }

    // Obtiene el nombre configurado en la identidad del dispositivo MikroTik.
    private static string GetMikrotikName(MK mikrotik)
    {
        mikrotik.Send("/system/identity/getall");
        mikrotik.Send("/system/identity/print", true);
        List<string> response = mikrotik.Read();

        string? identity = response.FirstOrDefault(item => item.StartsWith("=name=", StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(identity)
            ? "MikroTik"
            : identity.Substring("=name=".Length);
    }

    // Cuenta los registros de IP Binding para devolver la misma informacion que la consulta web.
    private static int GetIpBindingsCount(MK mikrotik)
    {
        mikrotik.Send("/ip/hotspot/ip-binding/getall");
        mikrotik.Send("/ip/hotspot/ip-binding/print");
        mikrotik.Send("=.proplist=address", true);

        List<string> response = mikrotik.Read();
        return response.Count(item => !item.StartsWith("!done", StringComparison.Ordinal));
    }

    // Evita intentos locales cuando faltan datos esenciales de la configuracion del servidor.
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
