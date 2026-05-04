using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class FormPurchase
{
    private IntItemModel? SelectedStatus;
    private List<IntItemModel>? ListStatus;
    private List<Supplier>? Suppliers;
    private List<ProductStorage>? ProductStorages;

    private DateTime? DateMin = new DateTime(2020, 1, 1);
    private DateTime? DateStart = DateTime.Now;

    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public Purchase Purchase { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await LoadSupplier();
        await LoadProductStorage();
    }

    private void DatePurchaseChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e?.Value?.ToString(), out var parsed))
        {
            Purchase.PurchaseDate = parsed;
        }
    }

    private void DateFacturaChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e?.Value?.ToString(), out var parsed))
        {
            Purchase.FacuraDate = parsed;
        }
    }

    private async Task LoadSupplier()
    {
        var responseHTTP = await _repository.GetAsync<List<Supplier>>($"api/v1/combosData/ComboSupplier");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            return;
        }

        Suppliers = responseHTTP.Response;
    }

    private async Task LoadProductStorage()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductStorage>>($"api/v1/combosData/ComboStorage");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            return;
        }

        ProductStorages = responseHTTP.Response;
    }

    private void ProductStorageChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Purchase.ProductStorageId = selectedId;
        }
    }

    private void SuplierChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Purchase.SupplierId = selectedId;
        }
    }
}