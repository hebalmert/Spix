using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.ShareLoading;
using Spix.Domain.EntitesSoftSec;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Entities.SoftSecPage;

public partial class CreateUsuario
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    //Parameters

    [Parameter] public string? Title { get; set; }

    private Usuario Usuario = new() { Active = true };
    private string BaseUrl = "/api/v1/usuarios";
    private string BaseView = "/usuarios";
    private bool isLoading = false;

    private async Task Create()
    {
        isLoading = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Usuario);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) { isLoading = false; return; }

        isLoading = false;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo(BaseView);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}");
    }
}