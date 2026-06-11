using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;
using System.Security.Claims;

namespace Spix.AppFront.Pages;

public partial class DashBoard
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    private List<string> CurrentRoles = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadUserRoles();
    }

    private async Task LoadUserRoles()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        CurrentRoles = user.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
    }
}