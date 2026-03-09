using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.CarguePage;

public partial class CreateCargueDetails
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private CargueDetail CargueDetail = new();
    private FormCargueDetails? FormCargueDetails { get; set; }

    private string BaseUrl = "/api/v1/cargueDetails";
    private string BaseView = "/cargues/details";

    [Parameter] public Guid Id { get; set; }  //CargueId
    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        CargueDetail.CargueId = Id;
    }

    private async Task Create()
    {
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", CargueDetail);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"{BaseView}/{Id}");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}