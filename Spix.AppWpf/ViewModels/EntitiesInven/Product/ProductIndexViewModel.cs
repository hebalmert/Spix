using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Product;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Presenta las categorias y sus productos en un unico indice como el acordeon de Blazor.
public partial class ProductIndexViewModel : PagedListViewModel<ProductCategoryRowViewModel>
{
    private const int ChildPageSize = 100;

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/productcategories";

    public ProductIndexViewModel(
        IPagedEntityService<ProductCategoryRowViewModel> pagedEntityService,
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

    [RelayCommand]
    private async Task ToggleProductsAsync(ProductCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        if (category.IsExpanded)
        {
            category.IsExpanded = false;
            return;
        }

        foreach (var item in Items)
        {
            item.IsExpanded = false;
        }

        category.IsExpanded = true;
        await LoadProductsAsync(category);
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateProductCategoryDialogView>(
            "Crear categoria producto");

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La categoria de productos fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ProductCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = category.ProductCategoryId
        };

        var result = await _modalService.ShowAsync<EditProductCategoryDialogView>(
            "Editar categoria producto",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La categoria de productos fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ProductCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var confirmed = await _alertService.ConfirmAsync(
            "Eliminar categoria producto",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync(
            $"api/v1/productcategories/{category.ProductCategoryId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La categoria de productos fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task NewProductAsync(ProductCategoryRowViewModel? category)
    {
        if (category is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["ProductCategoryId"] = category.ProductCategoryId
        };

        var result = await _modalService.ShowAsync<CreateProductDialogView>(
            "Crear producto",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(category.ProductCategoryId);
        await _alertService.SuccessAsync("Guardado", "El producto fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductEntity? product)
    {
        if (product is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = product.ProductId
        };

        var result = await _modalService.ShowAsync<EditProductDialogView>(
            "Editar producto",
            parameters);

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadExpandedCategoryAsync(product.ProductCategoryId);
        await _alertService.SuccessAsync("Actualizado", "El producto fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductEntity? product)
    {
        if (product is null)
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

        var responseHttp = await _repository.DeleteAsync($"api/v1/products/{product.ProductId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await ReloadExpandedCategoryAsync(product.ProductCategoryId);
        await _alertService.SuccessAsync("Eliminado", "El producto fue eliminado correctamente.");
    }

    // Descarga solamente los productos de la categoria abierta para conservar la paginacion del indice.
    private async Task LoadProductsAsync(ProductCategoryRowViewModel category)
    {
        category.IsProductsLoading = true;
        category.ProductsMessage = string.Empty;

        try
        {
            var responseHttp = await _repository.GetAsync<List<ProductEntity>>(
                $"api/v1/products?guidId={category.ProductCategoryId}&page=1&recordsnumber={ChildPageSize}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            category.Products = new ObservableCollection<ProductEntity>(
                responseHttp.Response ?? new List<ProductEntity>());

            if (category.Products.Count == 0)
            {
                category.ProductsMessage = "No hay productos registrados en esta categoria.";
            }
        }
        catch (Exception exception)
        {
            category.ProductsMessage = exception.Message;
        }
        finally
        {
            category.IsProductsLoading = false;
        }
    }

    private async Task ReloadExpandedCategoryAsync(Guid productCategoryId)
    {
        var category = Items.FirstOrDefault(
            item => item.ProductCategoryId == productCategoryId);

        if (category is null)
        {
            await LoadAsync(CurrentPage);
            return;
        }

        category.IsExpanded = true;
        await LoadProductsAsync(category);
    }
}
