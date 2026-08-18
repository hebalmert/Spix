using Microsoft.Extensions.Caching.Memory;

namespace Spix.AppInfra.Caching;

// Toda la complejidad del cacheo vive aqui: los services solo envuelven su consulta.
// IMPORTANTE: usarlo SOLO en combos que NO dependen de la corporation. Los combos por empresa
// no se cachean aqui, para que nunca se puedan mezclar datos entre tenants.
public class ComboCache : IComboCache
{
    private const int CacheMinutes = 10;

    private readonly IMemoryCache _cache;

    public ComboCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> loadAsync)
    {
        if (_cache.TryGetValue(key, out T? cached) && cached != null)
        {
            return cached;
        }

        T result = await loadAsync();
        _cache.Set(key, result, TimeSpan.FromMinutes(CacheMinutes));

        return result;
    }

    // Para invalidar a mano cuando se edita un catalogo y no se quiere esperar los 10 minutos.
    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
