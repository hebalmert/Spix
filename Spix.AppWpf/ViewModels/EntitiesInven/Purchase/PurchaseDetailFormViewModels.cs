using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Purchase;

// Centraliza los selects y calculos de una linea de compra sin llevar reglas al formulario.
public abstract partial class PurchaseDetailFormViewModel : CrudFormViewModel<Spix.Domain.EntitiesInven.PurchaseDetail>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<ProductCategory> _categories = new();

    [ObservableProperty]
    private ObservableCollection<ProductEntity> _products = new();

    [ObservableProperty]
    private Guid _productCategoryId;

    [ObservableProperty]
    private decimal _unitCost;

    [ObservableProperty]
    private decimal _quantity = 1;

    [ObservableProperty]
    private bool _isInitializingForm;

    [ObservableProperty]
    private decimal _rateTax;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _total;

    protected override string BaseUrl => "api/v1/purchaseDetails";

    protected PurchaseDetailFormViewModel(
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

    protected override Spix.Domain.EntitiesInven.PurchaseDetail CreateEntity()
    {
        return new Spix.Domain.EntitiesInven.PurchaseDetail
        {
            Quantity = 1
        };
    }

    protected override string? GetValidationMessage()
    {
        if (Entity.ProductId == Guid.Empty)
        {
            return "Debes seleccionar el producto.";
        }

        if (Quantity <= 0)
        {
            return "La cantidad debe ser mayor que cero.";
        }

        if (UnitCost < 0)
        {
            return "El costo unitario debe ser valido.";
        }

        return null;
    }

    partial void OnUnitCostChanged(decimal value)
    {
        Entity.UnitCost = value;
        CalculateTotals();
    }

    partial void OnQuantityChanged(decimal value)
    {
        Entity.Quantity = value;
        CalculateTotals();
    }

    partial void OnRateTaxChanged(decimal value)
    {
        Entity.RateTax = value;
        CalculateTotals();
    }

    // Prepara las categorias antes de que el usuario seleccione un producto.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ProductCategory>>(
                "api/v1/productcategories/loadCombo");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Categories = new ObservableCollection<ProductCategory>(
                response.Response ?? new List<ProductCategory>());
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

    // Define la compra padre para una nueva linea antes de guardarla.
    public void SetPurchaseId(Guid purchaseId)
    {
        Entity.PurchaseId = purchaseId;
    }

    // Refresca los productos de la categoria seleccionada con el endpoint existente.
    public async Task ChangeCategoryAsync(Guid productCategoryId)
    {
        ProductCategoryId = productCategoryId;
        Entity.ProductId = Guid.Empty;
        Products = new ObservableCollection<ProductEntity>();
        await LoadProductsAsync(productCategoryId);
    }

    // Consulta el producto para copiar su impuesto y costo sugerido, igual que Blazor.
    public async Task ChangeProductAsync(Guid productId)
    {
        Entity.ProductId = productId;

        var response = await _repository.GetAsync<ProductEntity>($"api/v1/products/{productId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var product = response.Response;
        if (product is null)
        {
            return;
        }

        RateTax = product.Tax?.Rate ?? 0;
        Quantity = 1;

        if (RateTax == 0)
        {
            UnitCost = product.Costo;
            return;
        }

        UnitCost = product.Costo / ((RateTax / 100) + 1);
    }

    // Recupera la linea existente y sus dependencias antes de permitir cambios.
    public async Task LoadForEditAsync(Guid id)
    {
        IsInitializingForm = true;

        try
        {
            await LoadAsync(id);

            UnitCost = Entity.UnitCost;
            Quantity = Entity.Quantity;
            RateTax = Entity.RateTax;

            if (Entity.Product is null)
            {
                return;
            }

            ProductCategoryId = Entity.Product.ProductCategoryId;
            await LoadProductsAsync(ProductCategoryId);
        }
        finally
        {
            IsInitializingForm = false;
        }
    }

    // Conserva los totales visibles sincronizados con la cantidad, costo e impuesto.
    private void CalculateTotals()
    {
        Subtotal = Quantity * UnitCost;
        TaxAmount = RateTax == 0
            ? 0
            : (((RateTax / 100) + 1) * Subtotal) - Subtotal;
        Total = Subtotal + TaxAmount;
    }

    private async Task LoadProductsAsync(Guid productCategoryId)
    {
        if (productCategoryId == Guid.Empty)
        {
            return;
        }

        var response = await _repository.GetAsync<List<ProductEntity>>(
            $"api/v1/products/loadCombo/{productCategoryId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Products = new ObservableCollection<ProductEntity>(
            response.Response ?? new List<ProductEntity>());
    }
}

public partial class CreatePurchaseDetailDialogViewModel : PurchaseDetailFormViewModel
{
    public CreatePurchaseDetailDialogViewModel(
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

public partial class EditPurchaseDetailDialogViewModel : PurchaseDetailFormViewModel
{
    public EditPurchaseDetailDialogViewModel(
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
