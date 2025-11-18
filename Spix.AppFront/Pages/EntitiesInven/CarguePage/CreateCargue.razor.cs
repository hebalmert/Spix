using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.CarguePage;

public partial class CreateCargue
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Cargue Cargue = new();
    private FormCargue? formCargue { get; set; }

    private string BaseUrl = "/api/v1/transfers";
    private string BaseView = "/transfers";

    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        var responseHttp = await _repository.PostAsync<Cargue, Cargue>($"{BaseUrl}", Cargue);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Cargue = responseHttp.Response!;
        formCargue!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}/details/{Cargue.CargueId}");
    }

    private void Return()
    {
        formCargue!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}");
    }
}