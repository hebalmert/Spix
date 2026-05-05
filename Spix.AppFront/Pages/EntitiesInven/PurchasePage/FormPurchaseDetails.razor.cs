using CurrieTechnologies.Razor.SweetAlert2;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.AppInfra.UtilityTools;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class FormPurchaseDetails
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public PurchaseDetail PurchaseDetail { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<ProductCategory>? Categories;

    private List<Product>? Products = new();

    private Product? ItemProducto;
    private decimal Total;

    private string BaseComboProductCategory = "/api/v1/productcategories/loadCombo";
    private string BaseComboProduct = "/api/v1/products/loadCombo";


    protected override async Task OnInitializedAsync()
    {
        await LoadCategory();
        if (IsEditControl)
        {
            await LoadProducts(PurchaseDetail.Product!.ProductCategoryId);
        }
    }

    private async Task LoadCategory()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductCategory>>($"{BaseComboProductCategory}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        Categories = responseHTTP.Response;
    }

    private async Task CategoryChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            PurchaseDetail.ProductCategoryId = selectedId;
        }
        Products = new();
        await LoadProducts(selectedId);
    }

    private async Task LoadProducts(Guid Id) //Recibe la CategoryId
    {
        var responseHTTP = await _repository.GetAsync<List<Product>>($"{BaseComboProduct}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        Products = responseHTTP.Response;
        if (IsEditControl)
        {
            Total = DecimalHelper.FormatDecimal(PurchaseDetail.SubTotal);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ProductsChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            PurchaseDetail.ProductId = selectedId;
        }
        //Traerme el dato del producto
        var responseHTTP = await _repository.GetAsync<Product>($"api/v1/products/{selectedId}");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        ItemProducto = responseHTTP.Response;
        //Igualamos datos
        PurchaseDetail.RateTax = DecimalHelper.FormatDecimal(ItemProducto!.Tax!.Rate);
        if (PurchaseDetail.RateTax == 0)
        {
            if (ItemProducto.Costo > 0)
            {
                PurchaseDetail.UnitCost = ItemProducto.Costo;
                PurchaseDetail.Quantity = 1;
                Total = DecimalHelper.FormatDecimal(PurchaseDetail.UnitCost * PurchaseDetail.Quantity);
            }
        }
        else
        {
            decimal impuesto = ItemProducto!.Tax!.Rate;
            decimal costo = ItemProducto.Costo;
            decimal Precio = costo / ((impuesto / 100) + 1);
            PurchaseDetail.UnitCost = DecimalHelper.FormatDecimal(Precio);
            PurchaseDetail.Quantity = 1;
            Total = DecimalHelper.FormatDecimal(Precio * PurchaseDetail.Quantity);
        }
    }

    private void CalculoTotalUnit(decimal valor)
    {
        decimal costo = PurchaseDetail.Quantity;
        if (PurchaseDetail.Quantity > 0 && valor > 0)
        {
            Total = DecimalHelper.FormatDecimal(costo * valor);
            PurchaseDetail.UnitCost = DecimalHelper.FormatDecimal(valor);
            return;
        }
        return;
    }

    private void CalculoTotalCant(decimal valor)
    {
        decimal costo = PurchaseDetail.UnitCost;
        if (PurchaseDetail.UnitCost > 0 && valor > 0)
        {
            Total = DecimalHelper.FormatDecimal(costo * valor);
            PurchaseDetail.Quantity = DecimalHelper.FormatDecimal(valor);
            return;
        }
        return;
    }
}