namespace Spix.AppFront.Helper;

public interface ISessionServiceModel<T>
{
    T? Cached { get; }

    Task SetSessionAsync(T model, string key);

    Task<T?> LoadSessionAsync(string key);

    Task ClearSessionAsync(string key);
}