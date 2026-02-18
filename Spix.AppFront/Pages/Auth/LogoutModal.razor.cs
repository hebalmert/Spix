using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.AuthenticationProviders;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Auth;

public partial class LogoutModal
{
    [Inject] private ILocalStorageService _localStorage { get; set; } = null!;
    [Inject] private ISessionServiceModel<SessionModelDTO> _sessionModel { get; set; } = null!;
    [Inject] private ILoginService LoginService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private async Task ConfirmLogout()
    {
        await _sessionModel.ClearSessionAsync("SessionDTO");
        await _localStorage.RemoveItemAsync("lastActivity");
        await LoginService.LogoutAsync();

        Navigation.NavigateTo("/", forceLoad: true);
        _modalService.Close();
    }

    private void CloseModal()
    {
        _modalService.Close();
    }
}