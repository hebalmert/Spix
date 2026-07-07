using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Security.Claims;

namespace Spix.AppFront.Pages.Auth;

public partial class ChangePassword
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigation { get; set; } = null!;
    [Inject] private HttpResponseHandler _httpHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    private ChangePasswordDTO changePasswordDTO = new();
    private bool showCurrentPassword;
    private bool showNewPassword;
    private bool showConfirmPassword;
    private string CurrentPasswordInputType => showCurrentPassword ? "text" : "password";
    private string NewPasswordInputType => showNewPassword ? "text" : "password";
    private string ConfirmPasswordInputType => showConfirmPassword ? "text" : "password";
    private bool ShowPasswordMatch => !string.IsNullOrWhiteSpace(changePasswordDTO.Confirm);
    private bool PasswordsMatch => string.Equals(changePasswordDTO.NewPassword, changePasswordDTO.Confirm, StringComparison.Ordinal);

    private void ToggleCurrentPassword()
    {
        showCurrentPassword = !showCurrentPassword;
    }

    private void ToggleNewPassword()
    {
        showNewPassword = !showNewPassword;
    }

    private void ToggleConfirmPassword()
    {
        showConfirmPassword = !showConfirmPassword;
    }

    private void NewPasswordChanged(ChangeEventArgs e)
    {
        changePasswordDTO.NewPassword = e.Value?.ToString() ?? string.Empty;
    }

    private void ConfirmPasswordChanged(ChangeEventArgs e)
    {
        changePasswordDTO.Confirm = e.Value?.ToString() ?? string.Empty;
    }

    private async Task ChangePasswordAsync()
    {
        var responseHttp = await _repository.PostAsync("/api/v1/accounts/changePassword", changePasswordDTO);
        if (await _httpHandler.HandleErrorAsync(responseHttp)) return;
        _modalService.Close();
        await NavigateToDashboardByRoleAsync();
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.PasswordUpdateTitle)], Localizer[nameof(Resource.PasswordUpdateMsg)], SweetAlertIcon.Success);
    }

    private async Task ReturnAction()
    {
        _modalService.Close();
        await NavigateToDashboardByRoleAsync();
    }

    private async Task NavigateToDashboardByRoleAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var role = authState.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")
            ?.Value;

        var dashboardUrl = role switch
        {
            var r when string.Equals(r, UserType.Client.ToString(), StringComparison.OrdinalIgnoreCase) => "/client-dashboard",
            var r when string.Equals(r, UserType.Technician.ToString(), StringComparison.OrdinalIgnoreCase) => "/tech-dashboard",
            _ => "/dashboard"
        };

        _navigation.NavigateTo(dashboardUrl);
    }
}
