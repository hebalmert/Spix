using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.AppInfra.UtilityTools;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

//Formulario del renglon, compartido por Crear y Editar.
//Los combos llegan del backend con su neutro; aqui solo se pintan.
public partial class FormPurchaseDetails
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public PurchaseDetail PurchaseDetail { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private const string ComboCategoryUrl = "/api/v1/productcategories/loadCombo";
    private const string ComboProductUrl = "/api/v1/products/loadCombo";
    private const string ProductUrl = "/api/v1/products";

    private List<ProductCategory>? Categories;
    private List<Product>? Products = new();

    //Si el producto lleva seriales, la cantidad va entera (el cargue pide una MAC por unidad)
    private bool WithSerials;

    //El costo unitario se muestra con separadores de miles, igual que los totales de
    //abajo. Mientras el campo tiene el foco se muestra sin ellos, porque con las comas
    //no se puede escribir.
    private bool EditandoCosto;

    private string UnitCostText => EditandoCosto
        ? PurchaseDetail.UnitCost.ToString("0.00", CultureInfo.CurrentCulture)
        : PurchaseDetail.UnitCost.ToString("N2", CultureInfo.CurrentCulture);

    private void UnitCostChanged(ChangeEventArgs e)
    {
        EditandoCosto = false;

        //Se leen con la MISMA cultura con la que se pintan: si se formatea en una y se
        //lee en otra, 1,512,605.00 se vuelve basura. Los separadores se quitan a mano
        //por si vienen de un copiar y pegar.
        var cultura = CultureInfo.CurrentCulture;
        var texto = (e.Value?.ToString() ?? string.Empty)
            .Replace(cultura.NumberFormat.NumberGroupSeparator, string.Empty)
            .Trim();

        PurchaseDetail.UnitCost = decimal.TryParse(texto, NumberStyles.Number, cultura, out var valor)
            ? valor
            : 0;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();

        //Al editar, la categoria sale del producto del renglon
        if (IsEditControl && PurchaseDetail.Product is not null)
        {
            PurchaseDetail.ProductCategoryId = PurchaseDetail.Product.ProductCategoryId;
            WithSerials = PurchaseDetail.Product.WithSerials;
            await LoadProductsAsync(PurchaseDetail.ProductCategoryId);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<ProductCategory>>(ComboCategoryUrl);
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Categories = responseHttp.Response;
    }

    private async Task LoadProductsAsync(Guid categoryId)
    {
        var responseHttp = await _repository.GetAsync<List<Product>>($"{ComboProductUrl}/{categoryId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Products = responseHttp.Response;
        await InvokeAsync(StateHasChanged);
    }

    //Cambiar de categoria deja el producto sin elegir
    private async Task CategoryChanged(ChangeEventArgs e)
    {
        PurchaseDetail.ProductCategoryId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
        PurchaseDetail.ProductId = Guid.Empty;
        PurchaseDetail.RateTax = 0;
        WithSerials = false;

        await LoadProductsAsync(PurchaseDetail.ProductCategoryId);
    }

    //Al elegir producto se trae su tasa y se sugiere su ultimo costo, sin el impuesto
    private async Task ProductsChanged(ChangeEventArgs e)
    {
        PurchaseDetail.ProductId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
        PurchaseDetail.RateTax = 0;
        WithSerials = false;
        if (PurchaseDetail.ProductId == Guid.Empty) return;

        var responseHttp = await _repository.GetAsync<Product>($"{ProductUrl}/{PurchaseDetail.ProductId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        var product = responseHttp.Response!;
        var rate = product.Tax?.Rate ?? 0;

        PurchaseDetail.RateTax = DecimalHelper.FormatDecimal(rate);
        PurchaseDetail.UnitCost = DecimalHelper.FormatDecimal(product.Costo / ((rate / 100) + 1));
        if (PurchaseDetail.Quantity <= 0) PurchaseDetail.Quantity = 1;
        WithSerials = product.WithSerials;
    }
}
