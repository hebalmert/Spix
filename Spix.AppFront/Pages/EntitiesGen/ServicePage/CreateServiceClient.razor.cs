using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class CreateServiceClient
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ServiceClient ServiceClient = new() { Active = true };

    private string BaseUrl = "/api/v1/serviceclients";
    private string BaseView = "/serviceclients/details";
    private bool IsVisible = false;
    [Parameter] public Guid Id { get; set; }  //ServiceCategoryId
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsVisible = true;
        ServiceClient.ServiceCategoryId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", ServiceClient);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            _modalService.Close();
            _navigationManager.NavigateTo($"/serviceclients");
            return;
        }
        IsVisible = false;
        _modalService.Close();
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        _navigationManager.NavigateTo("/dashboard");
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _modalService.Close();
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}