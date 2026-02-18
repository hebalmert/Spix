namespace Spix.AppFront.Helpers.Security;

public interface ICryptoService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}