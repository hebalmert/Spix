using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class EditServiceClient
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private ServiceClient? ServiceClient;

    private string BaseUrl = "/api/v1/serviceclients";
    private string BaseView = "/serviceclients/details";

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceClient>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        ServiceClient = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", ServiceClient);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}/{Id}");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{ServiceClient!.ServiceCategoryId}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{ServiceClient!.ServiceCategoryId}");
    }
}