using CurrieTechnologies.Razor.SweetAlert2;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;
public partial class EditPurchaseDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private PurchaseDetail? PurchaseDetail;
    private FormPurchaseDetails? FormPurchaseDetails { get; set; }
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public int Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHTTP = await _repository.GetAsync<PurchaseDetail>($"/api/purchaseDetails/{Id}");
        // Centralizamos el manejo de errores
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        PurchaseDetail = responseHTTP.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHTTP = await _repository.PutAsync("/api/purchaseDetails", PurchaseDetail);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}