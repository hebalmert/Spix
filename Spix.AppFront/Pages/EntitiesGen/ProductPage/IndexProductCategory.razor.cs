using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class IndexProductCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const decimal LowStockLimit = 15;

    private string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;

    private const string baseUrl = "api/v1/productcategories";
    private const string baseUrlProducts = "api/v1/products";

    public List<ProductCategory>? ProductCategories { get; set; }
    public Dictionary<Guid, List<Product>> ProductsByCategoryId { get; set; } = new();
    public Guid? SelectedProductCategoryId { get; set; }
    public HashSet<Guid> LoadingProductCategoryIds { get; set; } = new();

    //Filtros del panel de productos (se resuelven en pantalla, sin volver al servidor)
    private string ProductChip { get; set; } = "all";
    private string ProductFilter { get; set; } = string.Empty;

    private ProductCategory? SelectedCategory =>
        ProductCategories?.FirstOrDefault(x => x.ProductCategoryId == SelectedProductCategoryId);

    private List<Product> SelectedProducts =>
        SelectedProductCategoryId is not null && ProductsByCategoryId.TryGetValue(SelectedProductCategoryId.Value, out var products)
            ? products
            : new List<Product>();

    private int LowStockCount => SelectedProducts.Count(x => x.TotalInventario > 0 && x.TotalInventario <= LowStockLimit);

    private int NoStockCount => SelectedProducts.Count(x => x.TotalInventario == 0);

    private List<Product> FilteredProducts
    {
        get
        {
            var list = SelectedProducts.AsEnumerable();

            list = ProductChip switch
            {
                "stock" => list.Where(x => x.TotalInventario > 0),
                "low" => list.Where(x => x.TotalInventario > 0 && x.TotalInventario <= LowStockLimit),
                "none" => list.Where(x => x.TotalInventario == 0),
                "serials" => list.Where(x => x.WithSerials),
                _ => list
            };

            return list.ToList();
        }
    }

    private static string StockCss(decimal stock) =>
        stock == 0 ? "is-none" : stock <= LowStockLimit ? "is-low" : "is-ok";

    private void SetProductChip(string chip) => ProductChip = chip;

    //El texto va al servidor: asi busca en TODOS los productos de la categoria, no solo en los cargados
    private async Task SetProductFilter(string value)
    {
        ProductFilter = value;

        if (SelectedProductCategoryId is not null)
        {
            await LoadProductsForCategory(SelectedProductCategoryId.Value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SelectCategoryAsync(Guid productCategoryId)
    {
        SelectedProductCategoryId = productCategoryId;
        ProductChip = "all";
        ProductFilter = string.Empty;

        if (!ProductsByCategoryId.ContainsKey(productCategoryId))
        {
            await LoadProductsForCategory(productCategoryId);
        }
    }

    private async Task LoadProductsForCategory(Guid productCategoryId)
    {
        LoadingProductCategoryIds.Add(productCategoryId);
        await InvokeAsync(StateHasChanged);

        var url = $"{baseUrlProducts}?guidId={productCategoryId}&page=1&recordsnumber=100";
        if (!string.IsNullOrWhiteSpace(ProductFilter))
        {
            url += $"&filter={Uri.EscapeDataString(ProductFilter)}";
        }
        var responseHttp = await _repository.GetAsync<List<Product>>(url);

        LoadingProductCategoryIds.Remove(productCategoryId);

        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/dasboard");
            return;
        }

        ProductsByCategoryId[productCategoryId] = responseHttp.Response ?? new List<Product>();
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditProductCategory);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Product)]}" }
            };
        }
        else
        {
            component = typeof(CreateProductCategory);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_Product)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private async Task ShowModalProductAsync(Guid productCategoryId, Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditProduct);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Product)]}" }
            };
        }
        else
        {
            component = typeof(CreateProduct);
            parameters = new Dictionary<string, object>
            {
                { "Id", productCategoryId },
                { "Title", $"{Localizer[nameof(Resource.Create_Product)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);
                SelectedProductCategoryId = productCategoryId;
                await LoadProductsForCategory(productCategoryId);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private void ShowModalDetailsAsync(Guid? id = null)
    {
        _navigationManager.NavigateTo($"/products/details/{id}");
    }

    private async Task ShowModalStockAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", $"{Localizer[nameof(Resource.Stock)]}" }
        };

        await _modalService.ShowAsync(typeof(ProductStockModal), parameters);
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<ProductCategory>>(url);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/dasboard");
            return;
        }

        ProductCategories = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        ProductsByCategoryId.Clear();
        LoadingProductCategoryIds.Clear();

        //Se conserva la categoria elegida si sigue en la lista; si no, se toma la primera
        var previous = SelectedProductCategoryId;
        SelectedProductCategoryId = ProductCategories?.Any(x => x.ProductCategoryId == previous) == true
            ? previous
            : ProductCategories?.FirstOrDefault()?.ProductCategoryId;

        await InvokeAsync(StateHasChanged);

        if (SelectedProductCategoryId is not null)
        {
            await LoadProductsForCategory(SelectedProductCategoryId.Value);
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);

        if (SelectedProductCategoryId == id)
        {
            SelectedProductCategoryId = null;
        }

        await Cargar(CurrentPage);
    }

    private async Task DeleteProductAsync(Guid productCategoryId, Guid productId)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlProducts}/{productId}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);

        SelectedProductCategoryId = productCategoryId;
        await Cargar(CurrentPage);
    }
}
