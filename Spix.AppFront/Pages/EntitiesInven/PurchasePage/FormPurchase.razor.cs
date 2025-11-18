using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class FormPurchase
{
    private EditContext _editContext = null!;

    private IntItemModel? SelectedStatus;
    private List<IntItemModel>? ListStatus;

    private Supplier? SelectedSupplier;
    private List<Supplier>? Suppliers;

    private ProductStorage? SelectedProductStorage;
    private List<ProductStorage>? ProductStorages;

    private DateTime? DateMin = new DateTime(2020, 1, 1);
    private DateTime? DateStart = DateTime.Now;

    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Purchase Purchase { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    public bool FormPostedSuccessfully { get; set; } = false;

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
        var responseHTTP = await _repository.GetAsync<List<Supplier>>($"api/v1/suppliers/loadCombo");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        Suppliers = responseHTTP.Response;
        if (IsEditControl == true)
        {
            SelectedSupplier = Suppliers!.Where(x => x.SupplierId == Purchase.SupplierId)
                .Select(x => new Supplier { SupplierId = x.SupplierId, Name = x.Name }).FirstOrDefault();
        }
    }

    private async Task LoadProductStorage()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductStorage>>($"api/v1/productStorages/loadCombo");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        ProductStorages = responseHTTP.Response;
        if (IsEditControl == true)
        {
            SelectedProductStorage = ProductStorages!.Where(x => x.ProductStorageId == Purchase.ProductStorageId)
                .Select(x => new ProductStorage { ProductStorageId = x.ProductStorageId, StorageName = x.StorageName }).FirstOrDefault();
        }
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

    private string GetDisplayName<T>(Expression<Func<T>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            var property = memberExpression.Member as PropertyInfo;
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name!;
                }
            }
        }
        return "Texto no definido";
    }
}