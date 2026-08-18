namespace Spix.AppInfra.Caching;

// Cache corto para los combos de catalogos GLOBALES (paises, ciudades, tipos, planes...).
// Son datos que casi no cambian y que se piden en cada formulario que abre cada usuario.
public interface IComboCache
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> loadAsync);

    void Remove(string key);
}
