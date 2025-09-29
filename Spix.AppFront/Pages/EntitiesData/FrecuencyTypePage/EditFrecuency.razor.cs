using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesData.FrecuencyTypePage;

public partial class EditFrecuency
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    //Parameters

    [Parameter] public int Id { get; set; } //FreucnecyId
    [Parameter] public string? Title { get; set; }

    //Local State

    private Frecuency? Frecuency;
    private const string BaseUrl = "/api/v1/frecuencies";
    private const string BaseView = "/frecuencies/detailfrecuencies";

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<Frecuency>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Frecuency = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Frecuency);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}/{Frecuency!.FrecuencyTypeId}");
    }

    private void Return()
    {
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}/{Frecuency!.FrecuencyTypeId}");
    }
}