using Microsoft.AspNetCore.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.AuthenticationProviders;
using Spix.AppFront.Helper;

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

        //Primero se pregunta si HAY sesion. Antes se recargaba la app en el landing en los dos
        //casos, asi que un usuario valido tambien salia expulsado: al terminar el login, si el
        //Router alcanzaba a evaluar la ruta con el estado anterior, esto lo devolvia al inicio y
        //habia que loguearse dos veces.
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authenticationState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            //Tiene sesion: no se le cierra ni se recarga la app. Se le manda a su dashboard por
            //navegacion normal. Si la ruta que no pudo ver ERA su dashboard, entonces si es que
            //su rol no alcanza: queda en el landing, pero con la sesion intacta.
            var suDashboard = DashboardRoute.For(DashboardRoute.RolesOf(user));

            NavigationManager.NavigateTo($"/{destino}" == suDashboard ? "/" : suDashboard);
            return;
        }

        //Anonimo de verdad: se recuerda a donde iba, se limpia lo que quede y al landing.
        if (!string.IsNullOrWhiteSpace(destino))
        {
            await LocalStorage.SetItemAsStringAsync(ReturnUrlKey, $"/{destino}");
        }

        await LoginService.LogoutAsync();

        NavigationManager.NavigateTo("/", forceLoad: true);
    }
}
