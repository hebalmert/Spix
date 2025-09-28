using System.Text;
using Spix.DomainLogic.ResponcesSec;

namespace Spix.AppFront.Helpers.Security;

public class CryptoService : ICryptoService
{
    private readonly string _key;

    public CryptoService(CryptoSetting config)
    {
        _key = config.Key ?? throw new ArgumentNullException(nameof(config.Key));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var keyBytes = Encoding.UTF8.GetBytes(_key);
        var inputBytes = Encoding.UTF8.GetBytes(plainText);
        var outputBytes = new byte[inputBytes.Length];

        for (int i = 0; i < inputBytes.Length; i++)
        {
            outputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Convert.ToBase64String(outputBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        var keyBytes = Encoding.UTF8.GetBytes(_key);
        var inputBytes = Convert.FromBase64String(cipherText);
        var outputBytes = new byte[inputBytes.Length];

        for (int i = 0; i < inputBytes.Length; i++)
        {
            outputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Encoding.UTF8.GetString(outputBytes);
    }
}