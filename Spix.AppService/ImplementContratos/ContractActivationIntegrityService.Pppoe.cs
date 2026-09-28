using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementContratos;

//El gemelo PPPoE de lo que el archivo principal hace con los ip-binding de HotSpot.
//
//LA DIFERENCIA QUE IMPORTA: en HotSpot alcanza con un set del binding a regular y el cliente
//cae al portal. En PPPoE hay que hacer DOS cosas y en este orden:
//
//   1) /ppp/secret/set =disabled=yes   -> impide la PROXIMA autenticacion
//   2) /ppp/active/remove =.id=        -> tumba la sesion que ya esta arriba
//
//Deshabilitar el secret NO tumba la sesion viva: el secret solo se consulta al autenticar.
//Si falta el paso 2, el contrato queda Suspendido y el cliente sigue navegando. Y si se
//hace al reves, el router del cliente reconecta en segundos y tampoco se corto nada.
//
//Es el mismo mecanismo que usa WISPHUB: deshabilitar el secret y eliminar el active connection.
public partial class ContractActivationIntegrityService
{
    private const string PrefijoRe = "!re";
    private const string PrefijoTrap = "!trap";

    //Lo que PPPoE exige de un contrato: su credencial y su queue de velocidad.
    //El queue es el mismo de HotSpot: la tasa de reuso no cambia por el tipo de acceso.
    private async Task<ActionResponse<bool>> ValidatePppoeAsync(Guid contractClientId)
    {
        var tienePppoe = await _context.ContractPppoes
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        var tieneQueue = await _context.ContractQues
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        if (!tienePppoe && !tieneQueue)
        {
            return Fail(_localizer[nameof(Resource.Pppoe_RequirementsMissing)]);
        }

        if (!tienePppoe)
        {
            return Fail(_localizer[nameof(Resource.Pppoe_CredentialRequired)]);
        }

        if (!tieneQueue)
        {
            return Fail(_localizer[nameof(Resource.Pppoe_QueueRequired)]);
        }

        return Success();
    }

    // ---------- Devolver el acceso ----------

    private async Task<ActionResponse<bool>> ActivatePppoeAsync(ContractClient contract)
    {
        return await ActivatePppoeAsync(new List<ContractClient> { contract });
    }

    private async Task<ActionResponse<bool>> ActivatePppoeAsync(IEnumerable<ContractClient> contracts)
    {
        return await EscribirPppoeAsync(contracts, habilitar: true);
    }

    // ---------- Quitar el acceso ----------

    private async Task<ActionResponse<bool>> SuspendPppoeAsync(ContractClient contract)
    {
        return await SuspendPppoeAsync(new List<ContractClient> { contract });
    }

    private async Task<ActionResponse<bool>> SuspendPppoeAsync(IEnumerable<ContractClient> contracts)
    {
        return await EscribirPppoeAsync(contracts, habilitar: false);
    }

    //Una sola conexion por equipo para todos sus contratos, igual que el corte de HotSpot.
    private async Task<ActionResponse<bool>> EscribirPppoeAsync(IEnumerable<ContractClient> contracts, bool habilitar)
    {
        var contractList = contracts
            .GroupBy(x => x.ContractClientId)
            .Select(x => x.First())
            .ToList();

        if (contractList.Count == 0)
        {
            return Success();
        }

        var contractIds = contractList.Select(x => x.ContractClientId).ToList();

        var credenciales = await _context.ContractPppoes
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Include(x => x.IpNet)
            .Where(x => contractIds.Contains(x.ContractClientId))
            .ToListAsync();

        //Tienen que estar TODAS, no al menos una. Si falta la de un contrato y se sigue
        //igual, ese cliente queda marcado Suspendido en la base y navegando en el equipo,
        //y el lote se reporta exitoso. Es la falla silenciosa que se quiere evitar.
        var sinCredencial = contractIds
            .Except(credenciales.Select(x => x.ContractClientId))
            .ToList();

        if (sinCredencial.Count > 0)
        {
            return Fail(_localizer["Pppoe_BatchIncomplete", sinCredencial.Count.ToString()]);
        }

        //Antes de tocar un solo equipo se comprueba que no falte nada: mas vale no empezar
        //que dejar la mitad de los clientes cortados y la otra mitad no.
        foreach (var credencial in credenciales)
        {
            if (credencial.Server?.IpNetwork?.Ip == null ||
                string.IsNullOrWhiteSpace(credencial.Usuario) ||
                string.IsNullOrWhiteSpace(credencial.MikrotikId))
            {
                return Fail(_localizer[nameof(Resource.Pppoe_CredentialIncomplete)]);
            }
        }

        var porServidor = credenciales
            .GroupBy(x => x.ServerId)
            .ToList();

        foreach (var delServidor in porServidor)
        {
            var server = delServidor.First().Server!;
            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                foreach (var credencial in delServidor)
                {
                    //PASO 1: el secret. Esto decide si puede volver a autenticar.
                    mikrotik.Send("/ppp/secret/set");
                    mikrotik.Send("=.id=" + credencial.MikrotikId);
                    mikrotik.Send("=disabled=" + (habilitar ? "no" : "yes"), true);

                    if (HuboTrap(mikrotik.Read()))
                    {
                        return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                    }

                    //PASO 2: tumbar la sesion que ya esta arriba.
                    //
                    //Al suspender es OBLIGATORIO: sin esto el cliente sigue navegando.
                    //Al reactivar tambien conviene, para que reconecte y tome el perfil bueno.
                    if (!TumbarSesion(mikrotik, credencial.Usuario, credencial.IpCliente ?? credencial.IpNet?.Ip))
                    {
                        return Fail(_localizer["Pppoe_SessionNotKilled", credencial.Usuario]);
                    }

                    credencial.PppoeAccessState = habilitar
                        ? PppoeAccessState.Activo
                        : PppoeAccessState.Corte;
                }
            }
            catch
            {
                return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                CerrarMk(mikrotik);
            }
        }

        return Success();
    }

    //Busca la sesion viva del usuario y la corta.
    //
    //Puede devolver VARIOS renglones si el perfil permite mas de una sesion, asi que se
    //tumban todos: quedarse con el primero deja al cliente navegando por la otra.
    //
    //Que no haya ninguno NO es un error: significa que el cliente ya estaba desconectado.
    //Es una carrera normal entre la consulta y el corte.
    //Tumba la sesion viva del cliente. Devuelve false si el equipo contesto un error
    //que NO sea "no such item".
    //
    //ES EL UNICO LUGAR QUE BUSCA POR NOMBRE, y no se puede hacer de otra manera: la
    //sesion de /ppp/active tiene su propio .id, que nace cuando el cliente se conecta y
    //cambia en cada reconexion, asi que no hay ningun id que Spix pueda guardar. El
    //secret, que es lo que Spix crea, si se toca siempre por su MikrotikId.
    //
    //Para no depender solo del nombre se cruza con la IP: el secret le fija al cliente su
    //remote-address, asi que la sesion tiene que venir con esa IP. Si viene con otra, algo
    //no cuadra y no se toca nada.
    private static bool TumbarSesion(MK mikrotik, string usuario, string? ipEsperada)
    {
        mikrotik.Send("/ppp/active/print");
        mikrotik.Send("=.proplist=.id,name,address");
        mikrotik.Send("?name=" + usuario, true);

        var respuesta = mikrotik.Read();

        //Si el print falla no se sabe si hay sesion viva: no se puede dar por cortado
        if (HuboTrap(respuesta)) return false;

        var ids = new List<string>();

        foreach (var sentence in respuesta)
        {
            if (!sentence.StartsWith(PrefijoRe)) continue;

            var campos = LeerCamposMk(sentence);

            if (!campos.TryGetValue(".id", out var id) || string.IsNullOrWhiteSpace(id)) continue;

            //La comprobacion de la IP es OBLIGATORIA, no "si se puede".
            //
            //Si no sabemos que IP esperar, o el equipo no devolvio el address, NO se corta:
            //seria tumbar una sesion identificada solo por nombre.
            if (string.IsNullOrWhiteSpace(ipEsperada)) return false;

            if (!campos.TryGetValue("address", out var ipSesion) || string.IsNullOrWhiteSpace(ipSesion)) return false;

            if (!string.Equals(ipSesion, ipEsperada, StringComparison.OrdinalIgnoreCase)) return false;

            ids.Add(id);
        }

        foreach (var id in ids)
        {
            mikrotik.Send("/ppp/active/remove");
            mikrotik.Send("=.id=" + id, true);

            //"no such item" aca SI es exito: el cliente se cayo solo entre el print y el
            //remove, que es una carrera normal. Cualquier otro trap es un error de verdad y
            //tragarlo significaria dar por cortada una sesion que sigue viva.
            foreach (var sentence in mikrotik.Read())
            {
                if (!sentence.StartsWith(PrefijoTrap)) continue;

                if (!sentence.Contains("no such item", StringComparison.OrdinalIgnoreCase)) return false;
            }
        }

        return true;
    }

    // ---------- Se puede hablar con los equipos ----------

    private async Task<ActionResponse<bool>> VerifyPppoeConnectionAsync(ContractClient contract)
    {
        return await VerifyPppoeServersConnectionAsync(new List<Guid> { contract.ContractClientId });
    }

    //El gemelo de VerifyHotSpotServersConnectionAsync: junta los equipos que no responden y
    //los nombra todos, para que el operador no los descubra de a uno.
    private async Task<ActionResponse<bool>> VerifyPppoeServersConnectionAsync(IEnumerable<Guid> contractClientIds)
    {
        var contractIds = contractClientIds.Distinct().ToList();
        if (contractIds.Count == 0)
        {
            return Success();
        }

        var servidores = await _context.ContractPppoes
            .AsNoTracking()
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Select(x => new
            {
                x.ServerId,
                ServerName = x.Server!.ServerName,
                ServerIp = x.Server.IpNetwork!.Ip,
                x.Server.Usuario,
                x.Server.Clave,
                x.Server.ApiPort
            })
            .ToListAsync();

        var caidos = new List<string>();

        var unicos = servidores
            .GroupBy(x => x.ServerId)
            .Select(x => x.First())
            .ToList();

        foreach (var server in unicos)
        {
            var descripcion = $"{server.ServerName} ({server.ServerIp})";

            if (string.IsNullOrWhiteSpace(server.ServerIp))
            {
                caidos.Add(descripcion);
                continue;
            }

            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.ServerIp, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    caidos.Add(descripcion);
                }
            }
            catch
            {
                caidos.Add(descripcion);
            }
            finally
            {
                CerrarMk(mikrotik);
            }
        }

        if (caidos.Count > 0)
        {
            var texto = string.Join(", ", caidos.Distinct());
            return Fail(_localizer["Pppoe_ServersOffline", texto]);
        }

        return Success();
    }

    // ---------- Ayudas del protocolo ----------

    //Parte una sentence "!re=.id=*3=name=juan" en sus pares. Se camina por pares y no por
    //posicion porque RouterOS no garantiza en que orden devuelve las propiedades.
    private static Dictionary<string, string> LeerCamposMk(string sentence)
    {
        var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var partes = sentence.Split('=');

        for (int i = 1; i + 1 < partes.Length; i += 2)
        {
            campos[partes[i]] = partes[i + 1];
        }

        return campos;
    }

    private static bool HuboTrap(List<string> respuesta)
    {
        return respuesta.Any(x => x.StartsWith(PrefijoTrap));
    }

    private static void CerrarMk(MK? mikrotik)
    {
        if (mikrotik == null) return;

        try
        {
            mikrotik.Close();
        }
        catch
        {
        }
    }
}
