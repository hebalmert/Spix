using System.Security.Cryptography;
using System.Text;

namespace Spix.AppInfra.UtilityTools;

public class UtilityTools : IUtilityTools
{
    // ============================================================
    // METODO ORIGINAL (flexible)
    // ============================================================
    public string GeneratePass(int longitud, string caracteres)
    {
        StringBuilder res = new();
        byte[] buffer = new byte[1];

        while (res.Length < longitud)
        {
            RandomNumberGenerator.Fill(buffer);
            int index = buffer[0] % caracteres.Length;
            res.Append(caracteres[index]);
        }

        return res.ToString();
    }

    // ============================================================
    // METODO ESTANDAR (caracteres seguros para Blazor/URLs/Base64)
    // ============================================================
    public string GeneratePass(int longitud)
    {
        const string safeChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789" +
            "#@$%*";

        return GeneratePass(longitud, safeChars);
    }
}
