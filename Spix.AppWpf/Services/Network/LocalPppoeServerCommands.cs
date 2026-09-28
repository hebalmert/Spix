using Spix.DomainLogic.MkDTOs;
using Spix.xNetwork.MkHelper;

namespace Spix.AppWpf.Services.Network;

public static class LocalPppoeServerCommands
{
    public static List<MkInterfaceDTO> ReadInterfaces(MK router)
    {
        router.Send("/interface/print");
        router.Send("=.proplist=.id,name,type", true);
        var response = router.Read();
        RequireSuccess(response);

        return response.Where(x => x.StartsWith("!re", StringComparison.Ordinal))
            .Select(ParseFields)
            .Where(x => x.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
            .Select(x => new MkInterfaceDTO
            {
                Name = x["name"],
                Text = x.TryGetValue("type", out var type) ? $"{x["name"]} ({type})" : x["name"]
            })
            .OrderBy(x => x.Name)
            .ToList();
    }

    public static (string ProfileId, string ServerId) Provision(MK router, PppoeServerLocalSetupDTO setup)
    {
        if (FindIds(router, "/interface/print", "name", setup.LanName).Count == 0)
        {
            throw new InvalidOperationException("La interfaz de clientes no existe en este MikroTik.");
        }

        if (FindIds(router, "/ppp/profile/print", "name", setup.ProfileName).Count != 0)
        {
            throw new InvalidOperationException("Ya existe un perfil PPPoE con ese nombre. No se modifico.");
        }

        if (FindIds(router, "/interface/pppoe-server/server/print", "interface", setup.LanName).Count != 0)
        {
            throw new InvalidOperationException("Ya existe un servidor PPPoE en esa interfaz. No se modifico.");
        }

        router.Send("/ppp/profile/add");
        router.Send("=name=" + setup.ProfileName);
        router.Send("=local-address=" + setup.LocalIp);
        router.Send("=only-one=yes", true);
        var profileId = ReadCreatedId(router);

        router.Send("/interface/pppoe-server/server/add");
        router.Send("=interface=" + setup.LanName);
        router.Send("=service-name=" + setup.ServiceName);
        router.Send("=default-profile=" + setup.ProfileName);
        router.Send("=one-session-per-host=yes");
        router.Send("=disabled=no", true);
        var serverId = ReadCreatedId(router);

        return (profileId, serverId);
    }

    //El contrario de Provision. El ORDEN no es negociable: el servidor PPPoE apunta al
    //perfil, asi que primero el servidor y despues el perfil. Al reves el equipo rechaza
    //el borrado porque el perfil esta en uso.
    //
    //Siempre por .id, nunca por nombre: si Spix no tiene el id de algo, ese algo no es de
    //Spix y no se toca.
    public static void Remove(MK router, string profileId, string serverId)
    {
        if (!string.IsNullOrWhiteSpace(serverId))
        {
            router.Send("/interface/pppoe-server/server/remove");
            router.Send("=.id=" + serverId, true);
            ReadRemoval(router);
        }

        if (!string.IsNullOrWhiteSpace(profileId))
        {
            router.Send("/ppp/profile/remove");
            router.Send("=.id=" + profileId, true);
            ReadRemoval(router);
        }
    }

    //La respuesta de un remove se lee pero NO se juzga: un !trap aqui significa "ese id ya
    //no esta", y si alguien lo borro a mano en el equipo, Spix igual tiene que poder
    //limpiar su espejo. Y ojo: un remove devuelve solo !done, sin =ret=.
    private static void ReadRemoval(MK router)
    {
        router.Read();
    }

    private static string ReadCreatedId(MK router)
    {
        var response = router.Read();
        RequireSuccess(response);
        var sentence = response.FirstOrDefault(x => x.StartsWith("!done=ret=", StringComparison.Ordinal));
        if (sentence == null || sentence.Length <= "!done=ret=".Length)
        {
            throw new InvalidOperationException("El MikroTik no devolvio el identificador del registro creado.");
        }
        return sentence["!done=ret=".Length..];
    }

    private static List<string> FindIds(MK router, string command, string field, string value)
    {
        router.Send(command);
        router.Send("=.proplist=.id");
        router.Send($"?{field}={value}", true);
        var response = router.Read();
        RequireSuccess(response);
        return response.Where(x => x.StartsWith("!re", StringComparison.Ordinal))
            .Select(x => ParseFields(x).GetValueOrDefault(".id"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }

    private static Dictionary<string, string> ParseFields(string sentence)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var parts = sentence.Split('=');
        for (var index = 1; index + 1 < parts.Length; index += 2)
        {
            fields[parts[index]] = parts[index + 1];
        }
        return fields;
    }

    private static void RequireSuccess(List<string> response)
    {
        if (response.Any(x => x.StartsWith("!trap", StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("El MikroTik rechazo la consulta o la configuracion PPPoE.");
        }
    }
}
