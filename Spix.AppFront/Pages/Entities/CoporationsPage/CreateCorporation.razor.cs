using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.Entities.CoporationsPage;

public partial class CreateCorporation
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    //Parameters

    [Parameter] public string? Title { get; set; }

    private Corporation _Corporation = new() { Active = true };

    private string BaseUrl = "/api/v1/corporations";
    private string BaseView = "/corporations";

    private async Task Create()
    {
        if (_Corporation.SoftPlanId == 0 || _Corporation.CountryId == 0)
        {
            await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_ValidationWarningTitle)], Localizer[nameof(Resource.msg_ValidationWarningMessage)], SweetAlertIcon.Warning);
            return;
        }
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", _Corporation);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo(BaseView);
    }

    private void Return()
    {
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}");
    }
}