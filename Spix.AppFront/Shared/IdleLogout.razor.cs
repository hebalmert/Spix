using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Spix.AppFront.AuthenticationProviders;
using System.Globalization;

namespace Spix.AppFront.Shared;

public partial class IdleLogout : IAsyncDisposable
{
    [Inject] private ILocalStorageService _localStorage { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILoginService _loginService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider _authStateProvider { get; set; } = null!;

    private const string LastActivityKey = "lastActivity";

    //Tiempo sin actividad tras el cual se cierra la sesion
    private static readonly TimeSpan IdleLimit = TimeSpan.FromMinutes(10);

    private DotNetObjectReference<IdleLogout>? dotNetRef;
    private bool isLoggingOut;

    protected override async Task OnInitializedAsync()
    {
        //Si se vuelve a abrir la app despues del limite, la sesion que restauro el refresh se cierra
        if (await IdleLimitExceededAsync())
        {
            await LogoutByIdleAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || isLoggingOut) return;

        dotNetRef = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("startIdleTracking", dotNetRef);
    }

    //Lo llama idle-tracker.js (como maximo una vez cada 5 segundos)
    [JSInvokable]
    public async Task ResetIdleTimer()
    {
        await _localStorage.SetItemAsync(LastActivityKey, DateTime.UtcNow.ToString("o"));
    }

    //Lo dispara el BlazorTimer cada minuto
    private async Task CheckIdle()
    {
        if (isLoggingOut || !await IdleLimitExceededAsync()) return;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated != true) return;

        await LogoutByIdleAsync();
    }

    //Se compara contra la ultima actividad GUARDADA, que comparten todas las pestanas: asi una pestana
    //quieta no cierra la sesion mientras el usuario trabaja en otra. Sin dato guardado no se expulsa
    //(el login lo escribe al entrar).
    private async Task<bool> IdleLimitExceededAsync()
    {
        var stored = await _localStorage.GetItemAsync<string>(LastActivityKey);
        if (!DateTime.TryParse(stored, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var last)) return false;

        return DateTime.UtcNow - last.ToUniversalTime() > IdleLimit;
    }

    private async Task LogoutByIdleAsync()
    {
        isLoggingOut = true;

        await _loginService.LogoutAsync();
        Navigation.NavigateTo("/", forceLoad: true);
    }

    public async ValueTask DisposeAsync()
    {
        //Si el layout se desmonta, se quitan los listeners para no llamar a una referencia ya liberada
        try
        {
            await JS.InvokeVoidAsync("stopIdleTracking");
        }
        catch (JSException)
        {
        }

        dotNetRef?.Dispose();
    }
}
