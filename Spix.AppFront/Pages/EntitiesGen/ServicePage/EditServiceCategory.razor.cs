using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class EditServiceCategory
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private ServiceCategory? ServiceCategory;

    private string BaseUrl = "/api/v1/servicecategories";
    private string BaseView = "/servicecategories";

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceCategory>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        ServiceCategory = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", ServiceCategory);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}");
    }
}