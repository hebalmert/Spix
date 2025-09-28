using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.Resources;
using Spix.DomainLogic.ResponcesSec;
using Spix.HttpService;

namespace Spix.AppFront.Pages.Auth;

public partial class ResetPassword
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigation { get; set; } = null!;
    [Inject] private HttpResponseHandler _httpHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private ResetPasswordDTO resetPasswordDTO = new();

    [Parameter, SupplyParameterFromQuery] public string token { get; set; } = string.Empty;

    private async Task ChangePasswordAsync()
    {
        resetPasswordDTO.Token = token;
        var responseHttp = await _repository.PostAsync("/api/v1/accounts/ResetPassword", resetPasswordDTO);
        if (await _httpHandler.HandleErrorAsync(responseHttp)) return;
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.PasswordUpdateTitle)], Localizer[nameof(Resource.PasswordUpdateMsg)], SweetAlertIcon.Success);
        _navigation.NavigateTo("/");
        await _modalService.ShowAsync<Login>();
    }
}