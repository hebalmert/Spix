namespace Spix.DomainLogic.Configuration;

/// <summary>
/// Resuelve configuración desde la base de datos cifrada y, durante la transición,
/// desde appsettings como respaldo.
/// </summary>
public interface ISecretStore
{
    string? Get(string key, string? fallback = null);

    T Bind<T>(string section)
        where T : class, new();

    void Invalidate();
}
