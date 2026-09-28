using Spix.xNetwork.MkHelper;

namespace Spix.AppWpf.Services.Network;

// RouterOS commands are intentionally executed by WPF, never by the v2 API.
public static class LocalPppoeCommands
{
    public static string Add(MK router, string username, string password, string profile,
        string clientIp, string comment)
    {
        if (FindIds(router, "/ppp/secret/print", "name", username).Count != 0)
        {
            throw new InvalidOperationException("Ese usuario PPPoE ya existe en el MikroTik. No se modifico.");
        }

        router.Send("/ppp/secret/add");
        router.Send("=name=" + username);
        router.Send("=password=" + password);
        router.Send("=service=pppoe");
        router.Send("=profile=" + profile);
        router.Send("=remote-address=" + clientIp);
        router.Send("=comment=" + comment, true);

        var response = router.Read();
        RequireSuccess(response);
        var id = response.FirstOrDefault(x => x.StartsWith("!done=ret=", StringComparison.Ordinal));
        if (id == null || id.Length <= "!done=ret=".Length)
        {
            throw new InvalidOperationException("El MikroTik no devolvio el identificador de la credencial.");
        }

        return id["!done=ret=".Length..];
    }

    public static void Update(MK router, string id, string oldUsername, string username,
        string password, string clientIp)
    {
        var duplicate = FindIds(router, "/ppp/secret/print", "name", username);
        if (duplicate.Any(x => !string.Equals(x, id, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Ese usuario PPPoE ya existe en el MikroTik. No se modifico.");
        }

        router.Send("/ppp/secret/set");
        router.Send("=.id=" + id);
        router.Send("=name=" + username);
        router.Send("=password=" + password, true);
        RequireSuccess(router.Read());

        if (!string.Equals(oldUsername, username, StringComparison.Ordinal))
        {
            KillSession(router, oldUsername, clientIp);
        }
    }

    public static void Remove(MK router, string id, string username, string clientIp)
    {
        router.Send("/ppp/secret/remove");
        router.Send("=.id=" + id, true);
        RequireSuccess(router.Read(), allowMissing: true);
        KillSession(router, username, clientIp);
    }

    public static void SetAccess(MK router, string id, string username, string clientIp, bool enabled)
    {
        router.Send("/ppp/secret/set");
        router.Send("=.id=" + id);
        router.Send("=disabled=" + (enabled ? "no" : "yes"), true);
        RequireSuccess(router.Read());

        if (!enabled)
        {
            KillSession(router, username, clientIp);
        }
    }

    private static void KillSession(MK router, string username, string clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
        {
            throw new InvalidOperationException("No se conoce la IP del contrato; no se puede desconectar la sesion.");
        }

        router.Send("/ppp/active/print");
        router.Send("=.proplist=.id,name,address");
        router.Send("?name=" + username, true);
        var response = router.Read();
        RequireSuccess(response);

        foreach (var sentence in response.Where(x => x.StartsWith("!re", StringComparison.Ordinal)))
        {
            var fields = ParseFields(sentence);
            if (!fields.TryGetValue("address", out var address) ||
                !string.Equals(address, clientIp, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("La sesion PPPoE no corresponde a la IP del contrato. No se desconecto.");
            }

            if (!fields.TryGetValue(".id", out var id) || string.IsNullOrWhiteSpace(id))
            {
                throw new InvalidOperationException("No se pudo identificar la sesion PPPoE.");
            }

            router.Send("/ppp/active/remove");
            router.Send("=.id=" + id, true);
            RequireSuccess(router.Read(), allowMissing: true);
        }
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

    private static void RequireSuccess(List<string> response, bool allowMissing = false)
    {
        foreach (var sentence in response.Where(x => x.StartsWith("!trap", StringComparison.Ordinal)))
        {
            if (allowMissing && sentence.Contains("no such item", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            throw new InvalidOperationException("El MikroTik rechazo la operacion PPPoE: " + sentence);
        }
    }
}
