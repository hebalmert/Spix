using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

public partial class FormTransfer
{
    private Usuario? SelectedUsuario;
    private List<Usuario>? Usuarios;

    private ProductStorage? SelectedProductStorage1;
    private List<ProductStorage>? ProductStorages1;

    private ProductStorage? SelectedProductStorage2;
    private List<ProductStorage>? ProductStorages2;

    //Tecnicos y usuarios en una sola lista: la arma el backend con su neutro
    private List<TextItemModel>? Receivers;

    private DateTime? DateMin = new DateTime(2024, 1, 1);
    private DateTime? DateStart = DateTime.Now;

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Transfer Transfer { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    public bool FormPostedSuccessfully { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadProductStorage1();
        await LoadProductStorage2();
        await LoadReceivers();
    }

    private void DateTransferChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e?.Value?.ToString(), out var parsed))
        {
            Transfer.DateTransfer = parsed;
        }
    }

    //El combo llega ARMADO del backend, con su [Seleccione Bodega] y filtrado por
    //corporacion. Antes pedia productStorages/loadCombo, una ruta que no existe.
    private async Task LoadProductStorage1()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductStorage>>($"api/v1/combosData/ComboStorage");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/transfers");
            return;
        }

        ProductStorages1 = responseHTTP.Response;
        if (IsEditControl == true)
        {
            SelectedProductStorage1 = ProductStorages1!.Where(x => x.ProductStorageId == Transfer.FromProductStorageId)
                .Select(x => new ProductStorage { ProductStorageId = x.ProductStorageId, StorageName = x.StorageName }).FirstOrDefault();
        }
    }

    private void ProductStorageChanged1(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Transfer.FromProductStorageId = selectedId;
        }
    }

    private async Task LoadProductStorage2()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductStorage>>($"api/v1/combosData/ComboStorage");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/transfers");
            return;
        }

        ProductStorages2 = responseHTTP.Response;
        if (IsEditControl == true)
        {
            SelectedProductStorage2 = ProductStorages2!.Where(x => x.ProductStorageId == Transfer.ToProductStorageId)
                .Select(x => new ProductStorage { ProductStorageId = x.ProductStorageId, StorageName = x.StorageName }).FirstOrDefault();
        }
    }

    private async Task LoadReceivers()
    {
        var responseHTTP = await _repository.GetAsync<List<TextItemModel>>("api/v1/transfers/loadComboReceivers");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            Receivers = new List<TextItemModel>();
            return;
        }

        Receivers = responseHTTP.Response;
    }

    private void ReceiverChanged(ChangeEventArgs e)
    {
        Transfer.ReceiverKey = e?.Value?.ToString();
    }

    private void ProductStorageChanged2(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Transfer.ToProductStorageId = selectedId;
        }
    }
}