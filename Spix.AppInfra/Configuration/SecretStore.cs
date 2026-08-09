using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spix.AppInfra.SecretProtection;
using Spix.DomainLogic.Configuration;

namespace Spix.AppInfra.Configuration;

/// <summary>
/// Lee la configuración global del SaaS desde SystemSettings.
/// Mientras una llave no exista en la base de datos, usa appsettings como respaldo.
/// </summary>
public class SecretStore : ISecretStore
{
    private readonly DataContext _context;
    private readonly ISecretProtector _protector;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string>? _cache;

    public SecretStore(
        DataContext context,
        ISecretProtector protector,
        IConfiguration configuration)
    {
        _context = context;
        _protector = protector;
        _configuration = configuration;
    }

    public string? Get(string key, string? fallback = null)
    {
        var settings = Load();

        if (settings.TryGetValue(key, out string? value) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string? fromConfiguration = _configuration[key];

        return string.IsNullOrWhiteSpace(fromConfiguration)
            ? fallback
            : fromConfiguration;
    }

    public T Bind<T>(string section)
        where T : class, new()
    {
        var settings = Load();
        var result = new T();

        foreach (var property in typeof(T).GetProperties())
        {
            if (!property.CanWrite || property.PropertyType != typeof(string))
            {
                continue;
            }

            string key = $"{section}:{property.Name}";
            string? value = settings.TryGetValue(key, out string? storedValue)
                ? storedValue
                : _configuration[key];

            if (value != null)
            {
                property.SetValue(result, value);
            }
        }

        return result;
    }

    public void Invalidate()
    {
        _cache = null;
    }

    private Dictionary<string, string> Load()
    {
        if (_cache != null)
        {
            return _cache;
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var rows = _context.SystemSettings
            .AsNoTracking()
            .ToList();

        foreach (var row in rows)
        {
            string? value = _protector.Unprotect(row.Value);

            if (!string.IsNullOrWhiteSpace(value))
            {
                values[row.Key] = value;
            }
        }

        _cache = values;

        return _cache;
    }
}
