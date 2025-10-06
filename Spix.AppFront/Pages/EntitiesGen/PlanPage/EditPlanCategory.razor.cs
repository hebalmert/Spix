using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.PlanPage;

public partial class EditPlanCategory
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private PlanCategory? PlanCategory;

    private string BaseUrl = "/api/v1/plancategories";
    private string BaseView = "/plancategories";

    [Parameter] public Guid Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<PlanCategory>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        PlanCategory = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", PlanCategory);
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