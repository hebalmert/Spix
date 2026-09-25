using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppWpf.NetHelper;

// Comprueba DESDE ESTE EQUIPO la identidad y los IP bindings del MikroTik.
public class MkConnectionControl : IMkConnectionControl
{
    public async Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(
        string? ip,
        int apiPort,
        string? usuario,
        string? clave)
    {
        string? validationMessage = GetValidationMessage(ip, apiPort, usuario, clave);
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
            return await Task.Run(() => CheckConnection(ip!, apiPort, usuario!, clave!));
        }
        catch (Exception exception)
        {
            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = false,
                Message = GetFriendlyConnectionMessage(exception)
            };
        }
    }

    // Ejecuta el protocolo TCP fuera del hilo visual para mantener WPF responsivo.
    private static ActionResponse<MkConnectionResultDTO> CheckConnection(
        string ip,
        int apiPort,
        string usuario,
        string clave)
    {
        MK? mikrotik = null;

        try
        {
            mikrotik = new MK(ip, apiPort);

            if (!mikrotik.Login(usuario, clave))
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
                Message = GetFriendlyConnectionMessage(exception)
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
    private static string? GetValidationMessage(string? ip, int apiPort, string? usuario, string? clave)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return "El servidor no tiene una direccion IP de red configurada.";
        }

        if (apiPort <= 0)
        {
            return "El servidor no tiene un puerto API valido.";
        }

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            return "El servidor no tiene credenciales MikroTik configuradas.";
        }

        return null;
    }

    // Convierte fallas TCP tecnicas en una indicacion util para quien opera el sistema.
    private static string GetFriendlyConnectionMessage(Exception exception)
    {
        return "El servidor no respondio. Verifique que este encendido, que la IP sea accesible desde este equipo y que el puerto API este habilitado.";
    }
}
