using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Spix.AppFront.AuthenticationProviders;

namespace Spix.AppFront.Shared;

public partial class IdleLogout
{
    [Inject] private ILocalStorageService _localStorage { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILoginService _loginService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider _authStateProvider { get; set; } = null!;

    private DateTime lastActivity = DateTime.Now;
    private readonly TimeSpan idleLimit = TimeSpan.FromMinutes(2); //ajustado a 5 minutos
    private DotNetObjectReference<IdleLogout>? dotNetRef;

    protected override async Task OnInitializedAsync()
    {
        var stored = await _localStorage.GetItemAsync<string>("lastActivity");
        if (DateTime.TryParse(stored, out var last))
        {
            if (DateTime.Now - last > idleLimit)
            {
                await _loginService.LogoutAsync();
                Navigation.NavigateTo("/", forceLoad: true);
                return;
            }
            lastActivity = last;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("startIdleTracking", dotNetRef);
        }
    }

    [JSInvokable]
    public async Task ResetIdleTimer()
    {
        lastActivity = DateTime.Now;
        await _localStorage.SetItemAsync("lastActivity", lastActivity.ToString("o"));
    }

    private async Task CheckIdle()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? false)
            return;

        if (DateTime.Now - lastActivity > idleLimit)
        {
            await _loginService.LogoutAsync();
            Navigation.NavigateTo("/", forceLoad: true);
        }
    }

    public void Dispose()
    {
        dotNetRef?.Dispose();
    }
}