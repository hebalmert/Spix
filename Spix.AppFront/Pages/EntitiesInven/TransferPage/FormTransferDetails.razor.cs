using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;
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

    //La categoria elegida vive aqui y no en TransferDetails.Product: al crear una linea
    //ese Product viene en null y el select reventaba al pintarse.
    private Guid SelectedCategoryId;

    //Los equipos que se van a mover. Solo aplica a los productos CON serial: en esos el
    //equipo es la unidad, asi que la cantidad es cuantos se eligieron.
    public HashSet<Guid> SelectedSerials { get; } = new();

    private List<GuidItemModel>? AvailableSerials;

    private bool ProductWithSerials;

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
        if (IsEditControl && TransferDetails.Product is not null)
        {
            SelectedCategoryId = TransferDetails.Product.ProductCategoryId;
            await LoadProducts(SelectedCategoryId);
        }
    }

    private async Task LoadCategory()
    {
        var responseHTTP = await _repository.GetAsync<List<ProductCategory>>($"api/v1/productcategories/loadCombo");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            return;
            return;
        }

        Categories = responseHTTP.Response;
        if (IsEditControl && TransferDetails.Product is not null)
        {
            SelectedCategory = Categories!.Where(x => x.ProductCategoryId == TransferDetails.Product.ProductCategoryId)
                .Select(x => new ProductCategory { ProductCategoryId = x.ProductCategoryId, Name = x.Name }).FirstOrDefault();
        }
    }

    private async Task CategoryChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            SelectedCategoryId = selectedId;
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
            return;
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

        //Si el producto lleva serial hay que elegir los equipos, no escribir una cantidad
        ProductWithSerials = Products?.FirstOrDefault(x => x.ProductId == selectedId)?.WithSerials ?? false;
        SelectedSerials.Clear();
        AvailableSerials = null;

        if (ProductWithSerials)
        {
            await LoadSerials(selectedId);
        }

        //Traerme el dato del producto
        var responseHTTP = await _repository.GetAsync<TransferStockDTO>($"api/v1/productStocks/transferStock?TransferId={TransferDetails.TransferId}&ProductId={selectedId}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
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

    //Los seriales que se pueden mover: disponibles en la bodega de ORIGEN y sin reservar
    private async Task LoadSerials(Guid productId)
    {
        var url = $"api/v1/transferDetails/serials/available?transferId={TransferDetails.TransferId}&productId={productId}";
        if (IsEditControl)
        {
            url += $"&transferDetailsId={TransferDetails.TransferDetailsId}";
        }

        var responseHTTP = await _repository.GetAsync<List<GuidItemModel>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            return;
        }

        AvailableSerials = responseHTTP.Response ?? new();

        //Al editar se marcan los que la linea ya tenia
        if (!IsEditControl)
        {
            return;
        }

        var yaTiene = await _repository.GetAsync<List<GuidItemModel>>(
            $"api/v1/transferDetails/serials/line/{TransferDetails.TransferDetailsId}");

        if (await _responseHandler.HandleErrorAsync(yaTiene))
        {
            return;
        }

        foreach (var serial in yaTiene.Response ?? new())
        {
            SelectedSerials.Add(serial.Value);
        }

        TransferDetails.Quantity = SelectedSerials.Count;
    }

    //Marcar o desmarcar un equipo: la cantidad sigue a la cuenta
    private void ToggleSerial(Guid id, bool marcado)
    {
        if (marcado)
        {
            SelectedSerials.Add(id);
        }
        else
        {
            SelectedSerials.Remove(id);
        }

        TransferDetails.Quantity = SelectedSerials.Count;
    }
}
