using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class CreateServiceClient
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private ServiceClient ServiceClient = new();

    private string BaseUrl = "/api/v1/serviceclients";
    private string BaseView = "/serviceclients/details";

    [Parameter] public Guid Id { get; set; }  //ServiceCategoryId

    private async Task Create()
    {
        ServiceClient.ServiceCategoryId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", ServiceClient);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/serviceclients");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}