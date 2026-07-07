using Microsoft.Extensions.Options;
using Spix.DomainLogic.SettingModels;
using System.Security.Cryptography;
using System.Text;

namespace Spix.AppInfra.SecretProtection;

public class AesSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesSecretProtector(IOptions<SecretProtectionSettings> secretProtectionSettings,
        IOptions<JwtKeySetting> jwtKeySetting)
    {
        var seed = secretProtectionSettings.Value.EncryptionKey;
        if (string.IsNullOrWhiteSpace(seed))
        {
            seed = jwtKeySetting.Value.jwtKey;
        }

        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new InvalidOperationException("Encryption key is not configured.");
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    }

    public string? Protect(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] plainBytes = Encoding.UTF8.GetBytes(value);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string? Unprotect(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        byte[] fullCipher = Convert.FromBase64String(value);
        byte[] iv = fullCipher.Take(16).ToArray();
        byte[] cipherBytes = fullCipher.Skip(16).ToArray();

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
