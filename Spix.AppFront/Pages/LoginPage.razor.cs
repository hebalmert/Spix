using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.DomainLogic.EnumTypes;
using System.Security.Claims;

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
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            _navigationManager.NavigateTo(GetDashboardUrl(roles));
        }
        else
        {
            _checkingAuth = false;
        }
    }

    private static string GetDashboardUrl(IEnumerable<string> roles)
    {
        if (roles.Any(x => string.Equals(x, UserType.Client.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/client-dashboard";
        }

        if (roles.Any(x => string.Equals(x, UserType.Technician.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            return "/tech-dashboard";
        }

        return "/dashboard";
    }
}
