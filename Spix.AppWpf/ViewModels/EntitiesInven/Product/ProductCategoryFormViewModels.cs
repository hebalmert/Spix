using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Comparte el CRUD de categorias de productos entre crear y editar.
public abstract class ProductCategoryFormViewModel : CrudFormViewModel<ProductCategory>
{
    protected override string BaseUrl => "api/v1/productcategories";

    protected ProductCategoryFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override ProductCategory CreateEntity()
    {
        return new ProductCategory
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.Name)
            ? "Debes ingresar el nombre de la categoria."
            : null;
    }
}

public partial class CreateProductCategoryDialogViewModel : ProductCategoryFormViewModel
{
    public CreateProductCategoryDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditProductCategoryDialogViewModel : ProductCategoryFormViewModel
{
    public EditProductCategoryDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
