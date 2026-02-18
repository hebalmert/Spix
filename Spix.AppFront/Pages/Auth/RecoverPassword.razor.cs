using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.AppResponses;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Auth;

public partial class RecoverPassword
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigation { get; set; } = null!;
    [Inject] private HttpResponseHandler _httpHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private RecoveryPassDTO RecoveryPassDTO = new();

    private async Task SendRecoverPasswordEmailTokenAsync()
    {
        var responseHttp = await _repository.PostAsync("/api/v1/accounts/RecoverPassword", RecoveryPassDTO);
        if (await _httpHandler.HandleErrorAsync(responseHttp)) return;

        _modalService.Close();
        _navigation.NavigateTo("/");
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.EmailSentTitle)], Localizer[nameof(Resource.EmailSentMsg)], SweetAlertIcon.Success);
    }

    private void CloseModal()
    {
        _modalService.Close();
    }
}