using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class CreatePurchase
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Purchase Purchase = new();

    private FormPurchase? FormPurchase { get; set; }

    private string BaseUrl = "/api/v1/purchases";
    private string BaseView = "/purchases";

    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        Purchase.Status = PurchaseStatus.Pendiente;
        var responseHttp = await _repository.PostAsync<Purchase, Purchase>($"{BaseUrl}", Purchase);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Purchase = responseHttp.Response!;
        FormPurchase!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}/details/{Purchase.PurchaseId}");
    }

    private void Return()
    {
        FormPurchase!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}");
    }
}