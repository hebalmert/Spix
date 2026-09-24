using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Product;
using Spix.HttpService;
using ProductCategoryEntity = Spix.Domain.EntitiesGen.ProductCategory;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Productos: maestro-detalle, igual que en la web. Es el unico del catalogo que no filtra
// por activo sino por stock, y el unico con un tercer boton para ver las bodegas.
public partial class ProductIndexViewModel : CategoryDetailViewModel<ProductCategoryEntity, ProductEntity>
{
    // El mismo limite de la web: 15 o menos es stock bajo
    private const decimal LowStockLimit = 15;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/productcategories";

    protected override string ChildEndpoint => "api/v1/products";

    public ProductIndexViewModel(
        IPagedEntityService<ProductCategoryEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    protected override Guid GetCategoryId(ProductCategoryEntity category)
    {
        return category.ProductCategoryId;
    }

    public override string SelectedCategoryName => SelectedCategory?.Name ?? "Productos";

    // Los dos indicadores propios: lo que esta por acabarse y lo que ya se acabo
    public int LowStockCount =>
        Children.Count(item => item.TotalInventario > 0 && item.TotalInventario <= LowStockLimit);

    public int NoStockCount => Children.Count(item => item.TotalInventario == 0);

    protected override void NotifyKpis()
    {
        OnPropertyChanged(nameof(LowStockCount));
        OnPropertyChanged(nameof(NoStockCount));
    }

    protected override IEnumerable<ProductEntity> ApplyChip(IEnumerable<ProductEntity> children, string chip)
    {
        return chip switch
        {
            "stock" => children.Where(item => item.TotalInventario > 0),
            "low" => children.Where(item => item.TotalInventario > 0 && item.TotalInventario <= LowStockLimit),
            "none" => children.Where(item => item.TotalInventario == 0),
            "serials" => children.Where(item => item.WithSerials),
            _ => children
        };
    }

    protected override async Task<IReadOnlyCollection<ProductEntity>> GetChildrenAsync(string url)
    {
        var responseHttp = await _repository.GetAsync<List<ProductEntity>>(url);

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return Array.Empty<ProductEntity>();
        }

        return responseHttp.Response ?? new List<ProductEntity>();
    }

    // ===== La categoria =====

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateProductCategoryDialogView>("Crear categoria de productos");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de productos fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ProductCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditProductCategoryDialogView>(
            "Editar categoria de productos",
            new Dictionary<string, object> { ["Id"] = category.ProductCategoryId });

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de productos fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductCategoryEntity? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria de productos",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{Endpoint}/{category.ProductCategoryId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de productos fue eliminada correctamente.");
    }

    // ===== El producto =====

    [RelayCommand]
    private async Task NewChildAsync()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var categoryId = GetCategoryId(SelectedCategory);

        var result = await _modalService.ShowAsync<CreateProductDialogView>(
            "Crear producto",
            new Dictionary<string, object> { ["ProductCategoryId"] = categoryId });

        if (!result.Succeeded)
        {
            return;
        }

        //Se recarga tambien el padre porque cambia el contador de la fila
        await ReloadKeepingSelectionAsync(categoryId);
        await _alertService.SuccessAsync("Guardado", "El producto fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditChildAsync(ProductEntity? product)
    {
        if (product is null || SelectedCategory is null)
        {
            return;
        }

        var result = await _modalService.ShowAsync<EditProductDialogView>(
            "Editar producto",
            new Dictionary<string, object> { ["Id"] = product.ProductId });

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Actualizado", "El producto fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteChildAsync(ProductEntity? product)
    {
        if (product is null || SelectedCategory is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar producto",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{ChildEndpoint}/{product.ProductId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadKeepingSelectionAsync(GetCategoryId(SelectedCategory));
        await _alertService.SuccessAsync("Eliminado", "El producto fue eliminado correctamente.");
    }

    // Muestra en que bodegas esta repartido el stock. Solo consulta, no cambia nada.
    [RelayCommand]
    private async Task ShowStockAsync(ProductEntity? product)
    {
        if (product is null)
        {
            return;
        }

        await _modalService.ShowAsync<ProductStockDialogView>(
            "Existencias por bodega",
            new Dictionary<string, object> { ["Id"] = product.ProductId });
    }
}
