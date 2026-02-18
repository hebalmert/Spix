using Blazored.LocalStorage;
using Spix.AppFront.Helpers.Security;
using System.Text.Json;

namespace Spix.AppFront.Helper;

public class SessionServiceModel<T> : ISessionServiceModel<T>
{
    private readonly ILocalStorageService _localStorage;
    private readonly ICryptoService _crypto;

    public T? Cached { get; private set; }

    public SessionServiceModel(ILocalStorageService localStorage, ICryptoService crypto)
    {
        _localStorage = localStorage;
        _crypto = crypto;
    }

    public async Task SetSessionAsync(T model, string key)
    {
        var json = JsonSerializer.Serialize(model);
        var encrypted = _crypto.Encrypt(json);
        await _localStorage.SetItemAsync(key, encrypted);
        Cached = model;
    }

    public async Task<T?> LoadSessionAsync(string key)
    {
        var encrypted = await _localStorage.GetItemAsync<string>(key);
        if (string.IsNullOrEmpty(encrypted))
        {
            Cached = default;
            return default;
        }

        var decrypted = _crypto.Decrypt(encrypted);
        Cached = JsonSerializer.Deserialize<T>(decrypted);
        return Cached;
    }

    public async Task ClearSessionAsync(string key)
    {
        await _localStorage.RemoveItemAsync(key);
        Cached = default;
    }
}