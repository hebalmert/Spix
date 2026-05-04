using CurrieTechnologies.Razor.SweetAlert2;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class FormPurchaseDetails
{
    private EditContext _editContext = null!;

    private ProductCategory? SelectedCategory;
    private List<ProductCategory>? Categories;

    private Product? SelectedProduct;
    private List<Product>? Products = new();

    private Product? ItemProducto;
    private decimal Total;

    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public PurchaseDetail PurchaseDetail { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    public bool FormPostedSuccessfully { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategory();
        if (IsEditControl)
        {
            await LoadProducts(PurchaseDetail.ProductCategoryId);
        }
    }

    private async Task LoadCategory()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductCategory>>($"api/v1/productcategories/loadCombo");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        Categories = responseHTTP.Response;
        if (IsEditControl)
        {
            SelectedCategory = Categories!.Where(x => x.ProductCategoryId == PurchaseDetail.ProductCategoryId)
                .Select(x => new ProductCategory { ProductCategoryId = x.ProductCategoryId, Name = x.Name }).FirstOrDefault();
        }
    }

    private async Task CategoryChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            PurchaseDetail.ProductCategoryId = selectedId;
        }
        Products = new();
        SelectedProduct = new();
        await LoadProducts(selectedId);
    }

    private async Task LoadProducts(Guid Id) //Recibe la CategoryId
    {
        var responseHTTP = await _repository.GetAsync<List<Product>>($"api/v1/products/loadCombo/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        Products = responseHTTP.Response;
        if (IsEditControl)
        {
            SelectedProduct = Products!.Where(x => x.ProductId == PurchaseDetail.ProductId)
                .Select(x => new Product { ProductId = x.ProductId, ProductName = x.ProductName }).FirstOrDefault();
            Total = PurchaseDetail.SubTotal;
        }
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
        PurchaseDetail.RateTax = ItemProducto!.Tax!.Rate;
        if (PurchaseDetail.RateTax == 0)
        {
            if (ItemProducto.Costo > 0)
            {
                PurchaseDetail.UnitCost = ItemProducto.Costo;
                PurchaseDetail.Quantity = 1;
                Total = (decimal)(PurchaseDetail.UnitCost * PurchaseDetail.Quantity);
            }
        }
        else
        {
            decimal impuesto = ItemProducto!.Tax!.Rate;
            decimal costo = ItemProducto.Costo;
            decimal Precio = costo / ((impuesto / 100) + 1);
            PurchaseDetail.UnitCost = Precio;
            PurchaseDetail.Quantity = 1;
            Total = (decimal)(Precio * PurchaseDetail.Quantity);
        }
    }

    private void CalculoTotalUnit(decimal valor)
    {
        decimal costo = PurchaseDetail.Quantity;
        if (PurchaseDetail.Quantity > 0 && valor > 0)
        {
            Total = (costo * valor);
            PurchaseDetail.UnitCost = valor;
            return;
        }
        return;
    }

    private void CalculoTotalCant(decimal valor)
    {
        decimal costo = PurchaseDetail.UnitCost;
        if (PurchaseDetail.UnitCost > 0 && valor > 0)
        {
            Total = (costo * valor);
            PurchaseDetail.Quantity = valor;
            return;
        }
        return;
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