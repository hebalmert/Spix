using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.PlanPage;

public partial class CreatePlan
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Plan Plan = new();

    private string BaseUrl = "/api/v1/plans";
    private string BaseView = "/plans/details";

    [Parameter] public Guid Id { get; set; }  //ProductCategoryId

    private async Task Create()
    {
        Plan.PlanCategoryId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Plan);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/plancategories");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}