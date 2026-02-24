using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.HttpService;
using Spix.xLanguage.Resources;

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
    private bool IsVisible = false;
    private string BaseUrl = "/api/v1/corporations";
    private string BaseView = "/corporations";

    private async Task Create()
    {
        if (_Corporation.SoftPlanId == 0 || _Corporation.CountryId == 0)
        {
            await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_ValidationWarningTitle)], Localizer[nameof(Resource.msg_ValidationWarningMessage)], SweetAlertIcon.Warning);
            return;
        }
        IsVisible = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", _Corporation);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) { IsVisible = false; return; }

        IsVisible = false;
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}