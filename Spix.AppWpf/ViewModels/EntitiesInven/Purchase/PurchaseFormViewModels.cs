using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using SupplierEntity = Spix.Domain.EntitiesInven.Supplier;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Purchase;

// Carga proveedores y bodegas para crear o editar el encabezado de una compra.
public abstract partial class PurchaseFormViewModel : CrudFormViewModel<Spix.Domain.EntitiesInven.Purchase>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private ObservableCollection<SupplierEntity> _suppliers = new();

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private ObservableCollection<ProductStorage> _productStorages = new();

    protected override string BaseUrl => "api/v1/purchases";

    protected PurchaseFormViewModel(
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

    protected override Spix.Domain.EntitiesInven.Purchase CreateEntity()
    {
        return new Spix.Domain.EntitiesInven.Purchase
        {
            PurchaseDate = DateTime.UtcNow,
            FacuraDate = DateTime.UtcNow,
            Status = PurchaseStatus.Pendiente
        };
    }

    protected override string? GetValidationMessage()
    {
        if (Entity.SupplierId == Guid.Empty)
        {
            return "Debes seleccionar el proveedor.";
        }

        if (Entity.ProductStorageId == Guid.Empty)
        {
            return "Debes seleccionar la bodega.";
        }

        if (string.IsNullOrWhiteSpace(Entity.NroFactura))
        {
            return "Debes ingresar el numero de factura.";
        }

        return null;
    }

    // Descarga los selects necesarios antes de mostrar el formulario al usuario.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var supplierResponse = await _repository.GetAsync<List<SupplierEntity>>(
                "api/v1/combosData/ComboSupplier");
            if (await _responseHandler.HandleErrorAsync(supplierResponse))
            {
                return;
            }

            Suppliers = new ObservableCollection<SupplierEntity>(
                supplierResponse.Response ?? new List<SupplierEntity>());

            var storageResponse = await _repository.GetAsync<List<ProductStorage>>(
                "api/v1/combosData/ComboStorage");
            if (await _responseHandler.HandleErrorAsync(storageResponse))
            {
                return;
            }

            ProductStorages = new ObservableCollection<ProductStorage>(
                storageResponse.Response ?? new List<ProductStorage>());
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

    // Carga el encabezado existente despues de haber preparado sus selects.
    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
    }
}

public partial class CreatePurchaseDialogViewModel : PurchaseFormViewModel
{
    public CreatePurchaseDialogViewModel(
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

public partial class EditPurchaseDialogViewModel : PurchaseFormViewModel
{
    public EditPurchaseDialogViewModel(
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
