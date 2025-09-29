using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesData.FrecuencyTypePage;

public partial class EditFrecuencyType
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    //Parameters

    [Parameter] public int Id { get; set; }
    [Parameter] public string? Title { get; set; }

    //Local State

    private FrecuencyType? FrecuencyType;
    private const string BaseUrl = "/api/v1/frecuencytypes";
    private const string BaseView = "/frecuencytypes";

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<FrecuencyType>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        FrecuencyType = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", FrecuencyType);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
        _modalService.Close();
        _navigationManager.NavigateTo(BaseView);
    }

    private void Return()
    {
        _modalService.Close();
        _navigationManager.NavigateTo($"{BaseView}");
    }
}