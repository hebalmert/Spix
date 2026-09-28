using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.Helper;

namespace Spix.AppFront.Pages;

public partial class LoginPage
{
    [Inject] private AuthenticationStateProvider _AuthProvider { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    private bool _checkingAuth = true;

    protected override async Task OnInitializedAsync()
    {
        var authState = await _AuthProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            _navigationManager.NavigateTo(DashboardRoute.For(DashboardRoute.RolesOf(user)));
        }
        else
        {
            _checkingAuth = false;
        }
    }
}
