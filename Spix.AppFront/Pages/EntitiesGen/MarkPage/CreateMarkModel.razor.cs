using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.MarkPage;

public partial class CreateMarkModel
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private MarkModel MarkModel = new();

    private string BaseUrl = "/api/v1/marksmodels";
    private string BaseView = "/marksmodels/details";

    [Parameter] public Guid Id { get; set; }  //MarkId
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        MarkModel.MarkId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", MarkModel);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/marks");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}