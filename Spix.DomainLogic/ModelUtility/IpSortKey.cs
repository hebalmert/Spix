namespace Spix.DomainLogic.ModelUtility;

//Convierte una IPv4 en un numero para ordenar: 10.0.0.2 queda antes que 10.0.0.10.
//Lo usan IpNet e IpNetwork al asignar la IP, para guardar la clave en una columna con indice.
public static class IpSortKey
{
    public static long? From(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return null;

        var parts = ip.Split('.');
        if (parts.Length != 4) return null;

        long key = 0;
        foreach (var part in parts)
        {
            if (!byte.TryParse(part, out var octet)) return null;
            key = (key * 256) + octet;
        }

        return key;
    }
}
