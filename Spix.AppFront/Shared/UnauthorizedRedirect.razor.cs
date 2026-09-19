using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.AuthenticationProviders;

namespace Spix.AppFront.Shared;

public partial class UnauthorizedRedirect : ComponentBase
{
    [Inject] private ILoginService LoginService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = null!;

    //Se recuerda a donde iba el usuario para devolverlo despues del login (por ejemplo, el QR
    //de un documento firmado). Lo lee Login al entrar.
    public const string ReturnUrlKey = "ReturnUrl";

    private bool _isRedirecting;

    // Limpia cualquier sesion residual y envia las rutas protegidas sin acceso al landing publico.
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _isRedirecting)
        {
            return;
        }

        _isRedirecting = true;

        var destino = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        if (!string.IsNullOrWhiteSpace(destino))
        {
            await LocalStorage.SetItemAsStringAsync(ReturnUrlKey, $"/{destino}");
        }

        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authenticationState.User.Identity?.IsAuthenticated != true)
        {
            await LoginService.LogoutAsync();
        }

        NavigationManager.NavigateTo("/", forceLoad: true);
    }
}
