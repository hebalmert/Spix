using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesData.FrecuencyTypePage;

public partial class CreateFrecuency
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    //Parameters

    [Parameter] public int Id { get; set; }  //FrecuencyTypeId
    [Parameter] public string? Title { get; set; }

    private Frecuency Frecuency = new();

    private string BaseUrl = "/api/v1/frecuencies";
    private string BaseView = "/frecuencies/detailfrecuencies";

    private async Task Create()
    {
        Frecuency.FrecuencyTypeId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Frecuency);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _modalService.Close();
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}