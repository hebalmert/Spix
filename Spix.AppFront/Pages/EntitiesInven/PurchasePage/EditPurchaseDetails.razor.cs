using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class EditPurchaseDetails
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private PurchaseDetail? PurchaseDetail;
    private FormPurchaseDetails? FormPurchaseDetails { get; set; }

    [Parameter] public int Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var responseHTTP = await _repository.GetAsync<PurchaseDetail>($"/api/purchaseDetails/{Id}");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"/purchases/details/{Id}");
            return;
        }
        PurchaseDetail = responseHTTP.Response;
    }

    private async Task Edit()
    {
        var responseHTTP = await _repository.PutAsync("/api/purchaseDetails", PurchaseDetail);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"/purchases/details/{PurchaseDetail!.PurchaseId}");
            return;
        }

        FormPurchaseDetails!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"/purchases/details/{PurchaseDetail!.PurchaseId}");
    }

    private void Return()
    {
        FormPurchaseDetails!.FormPostedSuccessfully = true;
        _navigationManager.NavigateTo($"/purchases/details/{PurchaseDetail!.PurchaseId}");
    }
}