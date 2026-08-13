using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using MarkEntity = Spix.Domain.EntitiesGen.Mark;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Carga los combos dependientes del formulario de productos exactamente como el formulario Blazor.
public abstract partial class ProductFormViewModel : CrudFormViewModel<ProductEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<MarkEntity> _marks = new();

    [ObservableProperty]
    private ObservableCollection<MarkModel> _markModels = new();

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _taxes = new();

    protected override string BaseUrl => "api/v1/products";

    protected ProductFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    protected override ProductEntity CreateEntity()
    {
        return new ProductEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.ProductName))
        {
            return "Debes ingresar el nombre del producto.";
        }

        if (Entity.MarkId is null || Entity.MarkId == Guid.Empty)
        {
            return "Debes seleccionar una marca.";
        }

        if (Entity.MarkModelId is null || Entity.MarkModelId == Guid.Empty)
        {
            return "Debes seleccionar un modelo.";
        }

        if (Entity.TaxId == Guid.Empty)
        {
            return "Debes seleccionar un impuesto.";
        }

        if (Entity.Costo < 0 || Entity.Price < 0)
        {
            return "El costo y el precio deben ser validos.";
        }

        return null;
    }

    // Carga marcas e impuestos antes de habilitar los selects visibles en el formulario.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var marksResponse = await _repository.GetAsync<List<MarkEntity>>("api/v1/marks/loadCombo");
            if (await _responseHandler.HandleErrorAsync(marksResponse))
            {
                return;
            }

            Marks = new ObservableCollection<MarkEntity>(
                marksResponse.Response ?? new List<MarkEntity>());

            var taxesResponse = await _repository.GetAsync<List<GuidItemModel>>("api/v1/combosData/ComboTaxes");
            if (await _responseHandler.HandleErrorAsync(taxesResponse))
            {
                return;
            }

            Taxes = new ObservableCollection<GuidItemModel>(taxesResponse.Response ?? new List<GuidItemModel>());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Refresca los modelos correspondientes cuando el usuario escoge otra marca.
    public async Task ChangeMarkAsync(Guid markId)
    {
        Entity.MarkId = markId;
        Entity.MarkModelId = null;
        MarkModels = new ObservableCollection<MarkModel>();
        OnPropertyChanged(nameof(Entity));
        await LoadMarkModelsAsync(markId);
    }

    // Carga los modelos asociados a la marca de un producto existente o recien seleccionado.
    protected async Task LoadMarkModelsAsync(Guid markId)
    {
        if (markId == Guid.Empty)
        {
            return;
        }

        var responseHttp = await _repository.GetAsync<List<MarkModel>>(
            $"api/v1/marksmodels/loadCombo/{markId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        MarkModels = new ObservableCollection<MarkModel>(responseHttp.Response ?? new List<MarkModel>());
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);

        if (Entity.MarkId is Guid markId && markId != Guid.Empty)
        {
            await LoadMarkModelsAsync(markId);
        }
    }
}

public partial class CreateProductDialogViewModel : ProductFormViewModel
{
    public CreateProductDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    public void SetProductCategory(Guid productCategoryId)
    {
        Entity.ProductCategoryId = productCategoryId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditProductDialogViewModel : ProductFormViewModel
{
    public EditProductDialogViewModel(
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
