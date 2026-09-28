using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementContratos.ImplementContractControl;

//La credencial PPPoE de un contrato: el gemelo de ContractBindService, pero contra /ppp/secret.
//
//El secret lleva remote-address con la IP fija del contrato, y por eso el modulo de Queues
//sigue funcionando exactamente igual: el cliente PPPoE tiene la misma IP de siempre.
//
//El perfil NO se elige aca: es el del equipo, uno solo, creado al alistar el servidor. Y va
//sin rate-limit a proposito, porque la velocidad la maneja el Queue con la tasa de reuso.
public class ContractPppoeService : IContractPppoeService
{
    private const string PrefijoId = "!done=ret=";

    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractPppoeService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<ContractPppoe>> GetAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return Fallo<ContractPppoe>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Se busca por el CONTRATO, igual que su gemelo: hay una sola credencial por contrato
            var credencial = await _context.ContractPppoes
                .AsNoTracking()
                .Include(x => x.Server)
                .Include(x => x.IpNet)
                .FirstOrDefaultAsync(x => x.ContractClientId == id &&
                                          x.ContractClient!.CorporationId == user.CorporationId);

            //No tenerla todavia NO es un error: en DetailContractControl el contrato
            //empieza sin credencial y la tarjeta es la que ofrece crearla. Devolver un
            //fallo hacia que la pantalla sacara un "Not Found" al elegir el servidor.
            //Mismo criterio que ContractBindService, su gemelo de HotSpot.
            if (credencial == null) return Exito(new ContractPppoe());

            return Exito(SinClaveDelEquipo(credencial));
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractPppoe>(ex);
        }
    }

    public async Task<ActionResponse<ContractPppoe>> AddAsync(ContractPppoe modelo, string username)
    {
        //Validacion del modelo. El servidor y la IP NO se piden: se leen del contrato.
        if (modelo.ContractClientId == Guid.Empty ||
            string.IsNullOrWhiteSpace(modelo.Usuario))
        {
            return Fallo<ContractPppoe>(_localizer[nameof(Resource.Generic_InvalidModel)]);
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //El contrato tiene que ser de SU corporacion. Todo lo demas cuelga de aca.
            var contrato = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == modelo.ContractClientId &&
                                          x.CorporationId == user.CorporationId);

            if (contrato == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Una sola credencial por contrato
            var existe = await _context.ContractPppoes.AnyAsync(x => x.ContractClientId == modelo.ContractClientId);
            if (existe) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Pppoe_AlreadyExists)]);

            //El servidor y la IP se leen del CONTRATO, no se toman del modelo. Asi no hay
            //forma de crear una credencial apuntando al equipo o a la IP de otra empresa,
            //aunque alguien llame al endpoint a mano.
            var servidorId = await _context.ContractServers
                .AsNoTracking()
                .Where(x => x.ContractClientId == modelo.ContractClientId)
                .Select(x => (Guid?)x.ServerId)
                .FirstOrDefaultAsync();

            if (servidorId == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Server_Not_Found)]);

            var ipNetId = await _context.ContractIps
                .AsNoTracking()
                .Where(x => x.ContractClientId == modelo.ContractClientId)
                .Select(x => (Guid?)x.IpNetId)
                .FirstOrDefaultAsync();

            if (ipNetId == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //El usuario tiene que ser unico en el equipo: es con lo que autentica
            var usuario = modelo.Usuario.Trim().ToLowerInvariant();
            var repetido = await _context.ContractPppoes.AnyAsync(x => x.ServerId == servidorId && x.Usuario == usuario);
            if (repetido) return await FalloRollbackAsync<ContractPppoe>(_localizer["Pppoe_UserRepeated", usuario]);

            //El equipo, que tiene que estar alistado para PPPoE
            var server = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork)
                .FirstOrDefaultAsync(x => x.ServerId == servidorId && x.CorporationId == user.CorporationId);

            if (server == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Server_Not_Found)]);

            if (server.ControlMk != MikrotikControlType.PPPoE) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Server_NotPppoe)]);

            //Sin perfil no hay donde colgar el secret: el equipo todavia no se alisto
            if (string.IsNullOrWhiteSpace(server.PppProfileName)) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Pppoe_ServerNotReady)]);

            var ipCliente = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == ipNetId && x.CorporationId == user.CorporationId);

            if (ipCliente?.Ip == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Los ids que se guardan son los del contrato, no los que vinieron en el modelo
            modelo.ServerId = servidorId.Value;
            modelo.IpNetId = ipNetId.Value;

            //Si no viene clave, se genera una: es lo que hace cualquier sistema de ISP
            var clave = string.IsNullOrWhiteSpace(modelo.Clave) ? GenerarClave() : modelo.Clave.Trim();

            var comentario = $"{contrato.Client!.FirstName} {contrato.Client.LastName} - ({contrato.ControlContrato})";

            //Escritura en el equipo. Si esto falla no se guarda nada.
            MK? mikrotik = null;
            string? idmk;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);

                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //Si ya hay un secret con ese usuario en el equipo, NO se toca.
                //
                //La regla es la misma del IpBinding: todo lo que Spix crea le devuelve su
                //MikrotikId, y por ese id se edita y se borra. Si Spix no tiene el id de algo
                //que esta en el equipo, ese algo no es de Spix: puede ser de otro cliente o de
                //la configuracion previa del ISP. Reescribirlo le cortaria el servicio a un
                //tercero, asi que se avisa y lo resuelve el operador.
                //Si la consulta al equipo falla NO se sigue: un !trap significa que no se pudo
                //preguntar, no que el usuario no exista. Seguir seria crear a ciegas.
                if (!TryBuscarIdPorNombre(mikrotik, usuario, out var yaEnElEquipo))
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                if (yaEnElEquipo != null)
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer["Pppoe_UserExistsOnDevice", usuario]);
                }

                mikrotik.Send("/ppp/secret/add");
                mikrotik.Send("=name=" + usuario);
                mikrotik.Send("=password=" + clave);
                mikrotik.Send("=service=pppoe");
                mikrotik.Send("=profile=" + server.PppProfileName);
                mikrotik.Send("=remote-address=" + ipCliente.Ip);
                mikrotik.Send("=comment=" + comentario, true);

                idmk = CapturarId(mikrotik.Read());

                if (idmk == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            catch
            {
                return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                CerrarMk(mikrotik);
            }

            //Persistencia, con la foto de lo que quedo escrito
            modelo.Usuario = usuario;
            modelo.Clave = clave;
            modelo.MikrotikId = idmk;
            modelo.PppoeAccessState = PppoeAccessState.Activo;
            modelo.ServerName = server.ServerName;
            modelo.IpServer = server.IpNetwork.Ip;
            modelo.IpCliente = ipCliente.Ip;
            modelo.ProfileName = server.PppProfileName;

            _context.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Exito(modelo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractPppoe>(ex);
        }
    }

    //Solo se cambian el usuario y la clave. El servidor y la IP no se mueven: para eso se
    //borra y se vuelve a crear, igual que su gemelo.
    public async Task<ActionResponse<ContractPppoe>> UpdateAsync(ContractPppoe modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var actual = await _context.ContractPppoes
                .Include(x => x.Server)
                    .ThenInclude(x => x!.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractPppoeId == modelo.ContractPppoeId &&
                                          x.ContractClient!.CorporationId == user.CorporationId);

            if (actual == null) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (string.IsNullOrWhiteSpace(actual.MikrotikId)) return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Pppoe_CredentialIncomplete)]);

            var usuario = string.IsNullOrWhiteSpace(modelo.Usuario) ? actual.Usuario : modelo.Usuario.Trim().ToLowerInvariant();
            var clave = string.IsNullOrWhiteSpace(modelo.Clave) ? actual.Clave : modelo.Clave.Trim();

            if (usuario != actual.Usuario)
            {
                var repetido = await _context.ContractPppoes
                    .AnyAsync(x => x.ServerId == actual.ServerId &&
                                   x.Usuario == usuario &&
                                   x.ContractPppoeId != actual.ContractPppoeId);

                if (repetido) return await FalloRollbackAsync<ContractPppoe>(_localizer["Pppoe_UserRepeated", usuario]);
            }

            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(actual.Server!.IpNetwork!.Ip!, actual.Server.ApiPort);

                if (!mikrotik.Login(actual.Server.Usuario, actual.Server.Clave))
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //En un set alcanza el id y lo que cambia
                mikrotik.Send("/ppp/secret/set");
                mikrotik.Send("=.id=" + actual.MikrotikId);
                mikrotik.Send("=name=" + usuario);
                mikrotik.Send("=password=" + clave, true);

                //Estricto: en un set, "no such item" significa que el secret ya no existe y
                //que no se escribio nada. Aceptarlo dejaria la base diciendo que si.
                if (!RespuestaSinError(mikrotik.Read()))
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //(4) Si cambio el usuario hay que tumbar la sesion del nombre VIEJO.
                //
                //El set renombra el secret, pero la sesion que ya esta arriba sigue con el
                //nombre anterior. Si no se la tumba, una suspension posterior busca por el
                //nombre nuevo, no encuentra nada, y da el corte por hecho mientras el cliente
                //sigue navegando con la sesion vieja.
                if (usuario != actual.Usuario && !TumbarSesion(mikrotik, actual.Usuario, actual.IpCliente))
                {
                    return await FalloRollbackAsync<ContractPppoe>(_localizer["Pppoe_SessionNotKilled", actual.Usuario]);
                }
            }
            catch
            {
                return await FalloRollbackAsync<ContractPppoe>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                CerrarMk(mikrotik);
            }

            actual.Usuario = usuario;
            actual.Clave = clave;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Exito(SinClaveDelEquipo(actual));
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractPppoe>(ex);
        }
    }

    //Se quita del equipo y se borra. Primero se tumba la sesion: un secret borrado NO
    //desconecta al que ya esta adentro.
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return await FalloRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var actual = await _context.ContractPppoes
                .Include(x => x.Server)
                    .ThenInclude(x => x!.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractPppoeId == id &&
                                          x.ContractClient!.CorporationId == user.CorporationId);

            if (actual == null) return await FalloRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(actual.Server!.IpNetwork!.Ip!, actual.Server.ApiPort);

                if (!mikrotik.Login(actual.Server.Usuario, actual.Server.Clave))
                {
                    return await FalloRollbackAsync<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                //EL ORDEN IMPORTA, igual que en el corte: PRIMERO se borra el secret, para
                //que no pueda volver a autenticar, y DESPUES se tumba la sesion viva.
                //
                //Al reves el cliente reconecta en esa ventana, porque el secret todavia existe.
                if (!string.IsNullOrWhiteSpace(actual.MikrotikId))
                {
                    mikrotik.Send("/ppp/secret/remove");
                    mikrotik.Send("=.id=" + actual.MikrotikId, true);

                    //Que ya no exista es exito: alguien lo borro a mano. Otro trap es error, y
                    //tragarlo dejaria el secret en el equipo con la fila borrada de la base.
                    if (!RespuestaOk(mikrotik.Read()))
                    {
                        return await FalloRollbackAsync<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                    }
                }

                if (!TumbarSesion(mikrotik, actual.Usuario, actual.IpCliente))
                {
                    return await FalloRollbackAsync<bool>(_localizer["Pppoe_SessionNotKilled", actual.Usuario]);
                }
            }
            catch
            {
                return await FalloRollbackAsync<bool>(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                CerrarMk(mikrotik);
            }

            _context.Remove(actual);

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

    // ---------- Ayudas ----------

    //Tumba la sesion viva. false si el equipo contesto un error que no sea "no such item",
    //o si la sesion que aparecio no tiene la IP de este contrato.
    //
    //Es el unico lugar que busca por nombre: la sesion de /ppp/active tiene su propio .id,
    //que nace al conectarse y cambia en cada reconexion, asi que no hay id que guardar.
    //Por eso se cruza con la IP fija que el secret le asigno al cliente.
    private static bool TumbarSesion(MK mikrotik, string usuario, string? ipEsperada)
    {
        var ids = BuscarIdsSesion(mikrotik, usuario, ipEsperada);

        if (ids == null) return false;

        foreach (var idSesion in ids)
        {
            mikrotik.Send("/ppp/active/remove");
            mikrotik.Send("=.id=" + idSesion, true);

            //Que no exista es exito: se cayo solo entre el print y el remove
            if (!RespuestaOk(mikrotik.Read())) return false;
        }

        return true;
    }

    //false = el equipo contesto un error y no se sabe si existe.
    //true con id null = se pregunto bien y no existe.
    private static bool TryBuscarIdPorNombre(MK mikrotik, string usuario, out string? id)
    {
        var ids = BuscarIds(mikrotik, "/ppp/secret/print", "name", usuario);

        id = ids?.FirstOrDefault();

        return ids != null;
    }

    //Las sesiones vivas de ese usuario, comprobando que la IP sea la del contrato.
    //null si el equipo dio error o si aparecio una sesion con otra IP.
    private static List<string>? BuscarIdsSesion(MK mikrotik, string usuario, string? ipEsperada)
    {
        mikrotik.Send("/ppp/active/print");
        mikrotik.Send("=.proplist=.id,name,address");
        mikrotik.Send("?name=" + usuario, true);

        var respuesta = mikrotik.Read();

        if (respuesta.Any(x => x.StartsWith("!trap"))) return null;

        var ids = new List<string>();

        foreach (var sentence in respuesta)
        {
            if (!sentence.StartsWith("!re")) continue;

            var campos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var partes = sentence.Split('=');

            for (int i = 1; i + 1 < partes.Length; i += 2)
            {
                campos[partes[i]] = partes[i + 1];
            }

            if (!campos.TryGetValue(".id", out var id) || string.IsNullOrWhiteSpace(id)) continue;

            //La comprobacion de la IP es OBLIGATORIA, no "si se puede".
            //
            //Si no sabemos que IP esperar, o el equipo no devolvio el address, NO se corta:
            //estariamos tumbando una sesion identificada solo por nombre. Se aborta y el
            //operador se entera, que es mejor que cortarle el internet a otro cliente.
            if (string.IsNullOrWhiteSpace(ipEsperada)) return null;

            if (!campos.TryGetValue("address", out var ipSesion) || string.IsNullOrWhiteSpace(ipSesion)) return null;

            if (!string.Equals(ipSesion, ipEsperada, StringComparison.OrdinalIgnoreCase)) return null;

            ids.Add(id);
        }

        return ids;
    }

    //Los .id de las filas que matchean. null si el equipo contesto un error: no es lo
    //mismo "no hay ninguna" que "no se pudo preguntar".
    private static List<string>? BuscarIds(MK mikrotik, string comando, string campo, string valor)
    {
        mikrotik.Send(comando);
        mikrotik.Send("=.proplist=.id");
        mikrotik.Send($"?{campo}={valor}", true);

        var respuesta = mikrotik.Read();

        if (respuesta.Any(x => x.StartsWith("!trap"))) return null;

        var ids = new List<string>();

        foreach (var sentence in respuesta)
        {
            if (!sentence.StartsWith("!re")) continue;

            var partes = sentence.Split('=');

            for (int i = 1; i + 1 < partes.Length; i += 2)
            {
                if (partes[i].Equals(".id", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(partes[i + 1]))
                {
                    ids.Add(partes[i + 1]);
                }
            }
        }

        return ids;
    }

    //Para un REMOVE: "no such item" se acepta, porque la fila ya no estaba y eso es
    //justo lo que se queria conseguir.
    private static bool RespuestaOk(List<string> respuesta)
    {
        foreach (var sentence in respuesta)
        {
            if (!sentence.StartsWith("!trap")) continue;

            if (!sentence.Contains("no such item", StringComparison.OrdinalIgnoreCase)) return false;
        }

        return true;
    }

    //Para un SET o un ADD: cualquier !trap es error, incluido "no such item". Ahi ese
    //mensaje significa que no se escribio nada, no que ya estuviera hecho.
    private static bool RespuestaSinError(List<string> respuesta)
    {
        return !respuesta.Any(x => x.StartsWith("!trap"));
    }

    //La clave del MikroTik del equipo NO sale por el API. Abre todos los clientes de ese
    //servidor y estos endpoints los usa tambien el Auxiliar.
    //La clave PPPoE del cliente si: es suya y el operador se la tiene que entregar.
    private static ContractPppoe SinClaveDelEquipo(ContractPppoe credencial)
    {
        if (credencial.Server != null)
        {
            credencial.Server.Clave = string.Empty;
        }

        return credencial;
    }

    //Se valida el prefijo antes de cortar: si el equipo contesta "!trap", un Substring a
    //ciegas no revienta y guardaria basura como MikrotikId.
    private static string? CapturarId(List<string> respuesta)
    {
        foreach (var sentence in respuesta)
        {
            if (sentence.StartsWith("!trap")) return null;

            if (sentence.StartsWith(PrefijoId)) return sentence.Substring(PrefijoId.Length);
        }

        return null;
    }

    private static string GenerarClave()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 10);
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

    private static ActionResponse<T> Exito<T>(T resultado) => new() { WasSuccess = true, Result = resultado };

    private static ActionResponse<T> Fallo<T>(string mensaje) => new() { WasSuccess = false, Message = mensaje };

    private async Task<ActionResponse<T>> FalloRollbackAsync<T>(string mensaje)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fallo<T>(mensaje);
    }
}
