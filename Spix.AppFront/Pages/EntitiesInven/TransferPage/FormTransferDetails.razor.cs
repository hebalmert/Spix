using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using Spix.DomainLogic.EntitiesDTO;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

public partial class FormTransferDetails
{
    private ProductCategory? SelectedCategory;
    private List<ProductCategory>? Categories;

    private Product? SelectedProduct;
    private List<Product>? Products = new();

    private Product? ItemProducto;
    private decimal Total;

    private TransferStockDTO? TransferStockDTO;

    private decimal StockAvaible;

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public TransferDetails TransferDetails { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    public bool FormPostedSuccessfully { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategory();
        if (IsEditControl)
        {
            await LoadProducts(TransferDetails.ProductCategoryId);
        }
    }

    private async Task LoadCategory()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductCategory>>($"api/v1/productcategories/loadCombo");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/sells");
            return;
        }

        Categories = responseHTTP.Response;
        if (IsEditControl)
        {
            SelectedCategory = Categories!.Where(x => x.ProductCategoryId == TransferDetails.ProductCategoryId)
                .Select(x => new ProductCategory { ProductCategoryId = x.ProductCategoryId, Name = x.Name }).FirstOrDefault();
        }
    }

    private async Task CategoryChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            TransferDetails.ProductCategoryId = selectedId;
        }
        Products = new();
        SelectedProduct = new();
        await LoadProducts(selectedId);
    }

    private async Task LoadProducts(Guid Id) //Recibe la CategoryId
    {
        var responseHTTP = await _repository.GetAsync<List<Product>>($"api/v1/products/loadCombo/{Id}");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/sells");
            return;
        }
        Products = responseHTTP.Response;
        if (IsEditControl)
        {
            SelectedProduct = Products!.Where(x => x.ProductId == TransferDetails.ProductId)
                .Select(x => new Product { ProductId = x.ProductId, ProductName = x.ProductName }).FirstOrDefault();
        }
    }

    private async Task ProductsChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            TransferDetails.ProductId = selectedId;
        }

        //Traerme el dato del producto
        var responseHTTP = await _repository.GetAsync<TransferStockDTO>($"api/v1/productStocks/transferStock?TransferId={TransferDetails.TransferId}&ProductId={selectedId}");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo($"/transfers/details/{TransferDetails.TransferId}");
            return;
        }

        TransferStockDTO = responseHTTP.Response;
        //Igualamos datos
        StockAvaible = TransferStockDTO!.DiponibleOrigen;
    }

    private void CalculoTotalCant(decimal valor)
    {
        if (valor > StockAvaible)
        {
            TransferDetails.Quantity = StockAvaible;
            return;
        }
        TransferDetails.Quantity = valor;
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