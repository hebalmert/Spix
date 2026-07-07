namespace Spix.AppInfra.SecretProtection;

public interface ISecretProtector
{
    string? Protect(string? value);

    string? Unprotect(string? value);
}
