using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class CreatePurchaseDetails
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private PurchaseDetail PurchaseDetail = new();

    private FormPurchaseDetails? FormPurchaseDetails { get; set; }

    private string BaseUrl = "/api/v1/purchaseDetails";
    private string BaseView = "/purchases/details";

    [Parameter] public Guid Id { get; set; }  //PurchaseId
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        PurchaseDetail.PurchaseId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", PurchaseDetail);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"{BaseView}/{Id}");
            return;
        }

        FormPurchaseDetails!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        FormPurchaseDetails!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}