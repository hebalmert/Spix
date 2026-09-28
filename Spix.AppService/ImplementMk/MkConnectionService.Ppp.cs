using Microsoft.EntityFrameworkCore;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementMk;

//Lo que el EQUIPO necesita para trabajar en PPPoE, antes de que exista un solo contrato.
//
//Son dos cosas y nada mas: leerle las interfaces para poder elegir la de clientes sin
//escribirla a mano, y dejarle armado el servidor PPPoE con su perfil.
//
//Un solo perfil por equipo, a proposito: la velocidad la sigue manejando el Queue con la
//tasa de reuso, asi que el rate-limit del perfil va VACIO. Si se pusiera, RouterOS crearia
//ademas una cola dinamica por cliente y se limitaria dos veces.
public partial class MkConnectionService
{
    //Al leer una respuesta del equipo, el id solo viene despues de un "add" y con este
    //prefijo exacto. Despues de un set o un remove llega "!done" pelado, y un Substring
    //a ciegas ahi revienta o guarda basura.
    private const string PrefijoId = "!done=ret=";

    public async Task<ActionResponse<IEnumerable<MkInterfaceDTO>>> InterfacesComboAsync(Guid serverId, string username)
    {
        try
        {
            //Validacion
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<IEnumerable<MkInterfaceDTO>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == serverId &&
                                          x.CorporationId == user.CorporationId);

            if (server == null) return Fallo<IEnumerable<MkInterfaceDTO>>(_localizer[nameof(Resource.Server_Not_Found)]);

            //Lectura del equipo
            var lista = new List<MkInterfaceDTO>();
            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return Fallo<IEnumerable<MkInterfaceDTO>>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //Se pide .id aunque no se use: garantiza que la sentence nunca venga corta
                mikrotik.Send("/interface/print");
                mikrotik.Send("=.proplist=.id,name,type", true);

                foreach (var sentence in mikrotik.Read())
                {
                    if (sentence.StartsWith("!trap"))
                    {
                        return Fallo<IEnumerable<MkInterfaceDTO>>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                    }

                    if (!sentence.StartsWith("!re")) continue;

                    var campos = LeerCampos(sentence);

                    campos.TryGetValue("name", out var nombre);
                    if (string.IsNullOrWhiteSpace(nombre)) continue;

                    campos.TryGetValue("type", out var tipo);

                    lista.Add(new MkInterfaceDTO
                    {
                        Name = nombre,
                        Text = string.IsNullOrWhiteSpace(tipo) ? nombre : $"{nombre} ({tipo})"
                    });
                }
            }
            finally
            {
                CerrarSinRuido(mikrotik);
            }

            //La lista sale del backend lista para pintar, con el neutro en la posicion 0
            lista = lista.OrderBy(x => x.Name).ToList();

            lista.Insert(0, new MkInterfaceDTO
            {
                Name = string.Empty,
                Text = _localizer[nameof(Resource.Select_Interface)]
            });

            return Exito<IEnumerable<MkInterfaceDTO>>(lista);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<MkInterfaceDTO>>(ex);
        }
    }

    //El contrario del crear: borra el servidor PPPoE y su perfil DEL EQUIPO y deja el
    //espejo de Spix en blanco, para poder rehacer la configuracion (por ejemplo con otra
    //IP local).
    //
    //Solo se permite si el servidor NO tiene contratos PPPoE: los secrets de esos
    //clientes apuntan a este perfil, y borrarlo los deja a todos sin servicio.
    public async Task<ActionResponse<bool>> DeletePppoeServerAsync(Guid serverId, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers
                .Include(x => x.IpNetwork)
                .Include(x => x.PppLocalIpNet)
                .FirstOrDefaultAsync(x => x.ServerId == serverId &&
                                          x.CorporationId == user.CorporationId);

            if (server == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_Not_Found)]);

            if (string.IsNullOrWhiteSpace(server.PppServerMkId) &&
                string.IsNullOrWhiteSpace(server.PppProfileMkId))
            {
                return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppoeNotCreated)]);
            }

            //La guarda dura: con clientes provisionados no se toca
            var conContratos = await _context.ContractPppoes.AnyAsync(x => x.ServerId == serverId);
            if (conContratos) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppoeHasContracts)]);

            //Borrado en el equipo. El ORDEN no es negociable: el servidor PPPoE apunta al
            //perfil, asi que primero el servidor y despues el perfil. Al reves el equipo
            //rechaza el borrado porque el perfil esta en uso.
            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //Siempre por .id, nunca por nombre: si Spix no tiene el id de algo, ese
                //algo no es de Spix y no se toca.
                //
                //La respuesta de un remove se lee pero no se juzga: un !trap aqui significa
                //"ese id ya no esta", y si alguien lo borro a mano en el equipo, Spix igual
                //tiene que poder limpiar su espejo.
                if (!string.IsNullOrWhiteSpace(server.PppServerMkId))
                {
                    mikrotik.Send("/interface/pppoe-server/server/remove");
                    mikrotik.Send($"=.id={server.PppServerMkId}", true);
                    mikrotik.Read();
                }

                if (!string.IsNullOrWhiteSpace(server.PppProfileMkId))
                {
                    mikrotik.Send("/ppp/profile/remove");
                    mikrotik.Send($"=.id={server.PppProfileMkId}", true);
                    mikrotik.Read();
                }
            }
            finally
            {
                CerrarSinRuido(mikrotik);
            }

            //Persistencia: el equipo vuelve a quedar "sin crear"
            server.PppProfileName = null;
            server.PppProfileMkId = null;
            server.PppServiceName = null;
            server.PppServerMkId = null;

            //La IP local tambien se suelta. Dejarla elegida hacia ver la pantalla medio
            //configurada y no se sabia si el borrado habia pasado o no. Ahora el bloque de
            //PPPoE queda en blanco: sin IP, sin nombre de servicio y "sin crear".
            server.PppLocalIpNetId = null;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Exito(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Deja el equipo listo para recibir clientes PPPoE: crea el perfil y el servidor PPPoE
    //sobre la interfaz de clientes, y guarda los dos .id para no crear duplicados despues.
    public async Task<ActionResponse<bool>> CreatePppoeServerAsync(Guid serverId, string? serviceName, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var server = await _context.Servers
                .Include(x => x.IpNetwork)
                .Include(x => x.PppLocalIpNet)
                .FirstOrDefaultAsync(x => x.ServerId == serverId &&
                                          x.CorporationId == user.CorporationId);

            if (server == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_Not_Found)]);

            if (server.ControlMk != MikrotikControlType.PPPoE) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_NotPppoe)]);

            if (!string.IsNullOrWhiteSpace(server.PppServerMkId)) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppoeAlreadyExists)]);

            if (string.IsNullOrWhiteSpace(server.LanName)) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_LanNameRequired)]);

            if (server.PppLocalIpNet?.Ip == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Server_PppLocalIpRequired)]);

            //Los nombres: si el operador no los escribe, salen del nombre del equipo
            var perfil = Nombrar(server.PppProfileName, $"spix-{server.ServerName}");
            var servicio = Nombrar(serviceName ?? server.PppServiceName, $"spix-{server.ServerName}");

            //Escritura en el equipo
            MK? mikrotik = null;
            string? idPerfil;
            string? idServidor;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //La interfaz tiene que EXISTIR en el equipo. Sin esto se crearia el servidor
                //PPPoE sobre una interfaz inventada y nadie se enteraria hasta que un cliente
                //no pudiera conectarse.
                mikrotik.Send("/interface/print");
                mikrotik.Send("=.proplist=.id,name");
                mikrotik.Send($"?name={server.LanName}", true);

                if (!mikrotik.Read().Any(x => x.StartsWith("!re")))
                {
                    return await FalloRollbackAsync(_localizer["Server_LanNameNotFound", server.LanName]);
                }

                //Si el equipo YA tiene un perfil con ese nombre, no se toca.
                //
                //Misma regla que con el secret: si Spix no tiene el MikrotikId de algo, ese
                //algo no es de Spix. Reescribirle la IP local a un perfil que el ISP ya usaba
                //para otros clientes les cambiaria la configuracion a todos ellos.
                //Un !trap al preguntar significa que no se pudo consultar, no que no exista:
                //seguir de largo seria crear a ciegas sobre un equipo que no contesta bien.
                if (!TryBuscarId(mikrotik, "/ppp/profile/print", "name", perfil, out var perfilExistente))
                {
                    return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                if (perfilExistente != null)
                {
                    return await FalloRollbackAsync(_localizer["Pppoe_ProfileExistsOnDevice", perfil]);
                }

                //Y tampoco se reusa el servidor PPPoE que ya este en esa interfaz
                if (!TryBuscarId(mikrotik, "/interface/pppoe-server/server/print", "interface", server.LanName, out var servidorExistente))
                {
                    return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                if (servidorExistente != null)
                {
                    return await FalloRollbackAsync(_localizer["Pppoe_ServerExistsOnDevice", server.LanName]);
                }

                //El perfil: SIN rate-limit, porque la velocidad la maneja el Queue
                mikrotik.Send("/ppp/profile/add");
                mikrotik.Send($"=name={perfil}");
                mikrotik.Send($"=local-address={server.PppLocalIpNet.Ip}");
                mikrotik.Send("=only-one=yes", true);

                idPerfil = CapturarId(mikrotik.Read());
                if (idPerfil == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);

                //El servidor PPPoE sobre la interfaz de clientes
                mikrotik.Send("/interface/pppoe-server/server/add");
                mikrotik.Send($"=interface={server.LanName}");
                mikrotik.Send($"=service-name={servicio}");
                mikrotik.Send($"=default-profile={perfil}");
                mikrotik.Send("=one-session-per-host=yes");
                mikrotik.Send("=disabled=no", true);

                idServidor = CapturarId(mikrotik.Read());
                if (idServidor == null) return await FalloRollbackAsync(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                CerrarSinRuido(mikrotik);
            }

            //Persistencia: el equipo ya quedo escrito, ahora se guarda su espejo
            server.PppProfileName = perfil;
            server.PppProfileMkId = idPerfil;
            server.PppServiceName = servicio;
            server.PppServerMkId = idServidor;

            //NO se marca la IP local como asignada ni excluida: sale del pozo de RED y los
            //contratos sacan la suya del pozo de CLIENTES, que es otra tabla. Marcarla solo
            //hacia que el propio equipo no pudiera volver a elegir su IP de gestion.

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Exito(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Parte una sentence del tipo "!re=name=ether1=type=ether" en sus pares.
    //Se camina por pares y no por posicion porque RouterOS no garantiza el orden en que
    //devuelve las propiedades.
    private static Dictionary<string, string> LeerCampos(string sentence)
    {
        var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var partes = sentence.Split('=');

        for (int i = 1; i + 1 < partes.Length; i += 2)
        {
            campos[partes[i]] = partes[i + 1];
        }

        return campos;
    }

    //Busca una fila por un campo.
    //
    //false = el equipo contesto un error, asi que NO se sabe si existe.
    //true con id null = se pregunto bien y no existe.
    //
    //La diferencia importa: tratar un error como "no existe" hace que el alistado siga
    //adelante y cree cosas sobre un equipo que no esta contestando bien.
    private static bool TryBuscarId(MK mikrotik, string comando, string campo, string valor, out string? id)
    {
        id = null;

        mikrotik.Send(comando);
        mikrotik.Send("=.proplist=.id");
        mikrotik.Send($"?{campo}={valor}", true);

        foreach (var sentence in mikrotik.Read())
        {
            if (sentence.StartsWith("!trap")) return false;

            if (!sentence.StartsWith("!re")) continue;

            var campos = LeerCampos(sentence);

            if (campos.TryGetValue(".id", out var encontrado) && !string.IsNullOrWhiteSpace(encontrado))
            {
                id = encontrado;
            }
        }

        return true;
    }

    //El id de un add. Se valida el prefijo en vez de cortar a ciegas: si el equipo contesta
    //"!trap", un Substring(10) no revienta y guardaria basura como MikrotikId.
    private static string? CapturarId(List<string> respuesta)
    {
        foreach (var sentence in respuesta)
        {
            if (sentence.StartsWith("!trap")) return null;

            if (sentence.StartsWith(PrefijoId)) return sentence.Substring(PrefijoId.Length);
        }

        return null;
    }

    private static string Nombrar(string? elegido, string porDefecto)
    {
        var crudo = string.IsNullOrWhiteSpace(elegido) ? porDefecto : elegido;
        var limpio = new string(crudo.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).ToLowerInvariant();

        while (limpio.Contains("--")) limpio = limpio.Replace("--", "-");

        limpio = limpio.Trim('-');

        return limpio.Length > 50 ? limpio.Substring(0, 50) : limpio;
    }

    private static void CerrarSinRuido(MK? mikrotik)
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

    private static ActionResponse<T> Exito<T>(T resultado) => new() { WasSuccess = true, Result = resultado };

    private static ActionResponse<T> Fallo<T>(string mensaje) => new() { WasSuccess = false, Message = mensaje };

    private async Task<ActionResponse<bool>> FalloRollbackAsync(string mensaje)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fallo<bool>(mensaje);
    }
}
