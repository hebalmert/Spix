using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.Auth;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppFront.Shared;

public partial class AuthLinks
{
    [Inject] private ISessionServiceModel<SessionModelDTO> _sessionModel { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; } = null!;

    private bool mostrarModalLogout = false;
    private string? photoUser;
    private string? LogoCorp;
    private string? NameCorp;
    private SessionModelDTO? SessionModelDTO = new();

    protected override async Task OnParametersSetAsync()
    {
        var authenticationState = await AuthenticationStateTask;
        var claims = authenticationState.User.Claims.ToList();

        SessionModelDTO = await _sessionModel.LoadSessionAsync("SessionDTO");
        photoUser = SessionModelDTO!.PhotoBase64;
        LogoCorp = SessionModelDTO.LogoBase64;
    }

    private async Task ShowModalLogIn()
    {
        await _modalService.ShowAsync<Login>();
    }

    private async Task ShowModalLogOut()
    {
        await _modalService.ShowAsync<LogoutModal>();
    }

    private async Task ShowModalRecoverPassword()
    {
        await _modalService.ShowAsync<RecoverPassword>();
    }

    private async Task ShowModalCambiarClave()
    {
        await _modalService.ShowAsync<ChangePassword>();
    }
}