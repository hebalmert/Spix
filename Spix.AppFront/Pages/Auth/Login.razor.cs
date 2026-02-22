using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.AuthenticationProviders;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Spix.AppFront.Pages.Auth;

public partial class Login
{
    [Inject] private ISessionServiceModel<SessionModelDTO> _sessionModel { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigation { get; set; } = null!;
    [Inject] private ILoginService _loginService { get; set; } = null!;
    [Inject] private HttpResponseHandler _httpHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private LoginDTO loginDTO = new();
    private SessionModelDTO sessionModelDTO = new();
    private bool rememberMe;
    private bool isProcessing = false;
    private bool showPassword = false;
    private async Task LoginAsync()
    {
        isProcessing = true;

        var responseHttp = await _repository.PostAsync<LoginDTO, TokenDTO>("/api/v1/accounts/Login", loginDTO);
        if (await _httpHandler.HandleErrorAsync(responseHttp))
        {
            isProcessing = false;
            return;
        }

        var token = responseHttp.Response!.Token;
        await _loginService.LoginAsync(token);

        // Extraer claims directamente del token
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value ?? string.Empty;
        sessionModelDTO.PhotoBase64 = responseHttp.Response.PhotoBase64;
        sessionModelDTO.LogoBase64 = responseHttp.Response.LogoBase64;
        sessionModelDTO.Role = role;
        sessionModelDTO.Expiration = responseHttp.Response.Expiration;
        await _sessionModel.SetSessionAsync(sessionModelDTO, "SessionDTO");

        isProcessing = false;
        _navigation.NavigateTo("/dashboard");
    }

    private async Task OpenRecoverPasswordModal()
    {
        await _modalService.ShowAsync<RecoverPassword>();
    }

    private void TogglePasswordVisibility()
    {
        showPassword = !showPassword;
    }
}
