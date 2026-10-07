using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// Una linea del traslado: que producto se mueve y cuanto.
//
// Lo que diferencia esto de una linea de compra: aqui NO se inventa cantidad. Se le
// pregunta al Backend cuanto hay de ese producto en la bodega de ORIGEN y la cantidad
// se topa ahi. Trasladar mas de lo que hay dejaria el inventario en negativo.
public abstract partial class TransferDetailFormViewModel : CrudFormViewModel<TransferDetails>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;
    private readonly ModalService _modalService;

    [ObservableProperty]
    private ObservableCollection<ProductCategory> _categories = new();

    [ObservableProperty]
    private ObservableCollection<ProductEntity> _products = new();

    [ObservableProperty]
    private Guid _productCategoryId;

    [ObservableProperty]
    private decimal _quantity = 1;

    //Lo que hay del producto en la bodega de origen: es el techo de la cantidad
    [ObservableProperty]
    private decimal _stockAvailable;

    //Mientras se arma el formulario de edicion, el cambio de categoria NO debe
    //borrar el producto que el registro ya tenia
    [ObservableProperty]
    private bool _isInitializingForm;

    //Los equipos que se van a mover. Solo aplica a los productos CON serial: ahi el
    //equipo es la unidad, asi que la cantidad es cuantos se marcaron.
    [ObservableProperty]
    private ObservableCollection<SerialPickRow> _availableSerials = new();

    [ObservableProperty]
    private bool _productWithSerials;

    protected override string BaseUrl => "api/v2/transferDetails";

    protected TransferDetailFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
        _modalService = modalService;
    }

    protected override TransferDetails CreateEntity()
    {
        return new TransferDetails
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

        if (Quantity > StockAvailable)
        {
            return $"Solo hay {StockAvailable:N2} en la bodega de origen.";
        }

        return null;
    }

    // La cantidad nunca pasa de lo que hay en la bodega de origen
    partial void OnQuantityChanged(decimal value)
    {
        if (StockAvailable > 0 && value > StockAvailable)
        {
            Quantity = StockAvailable;
            return;
        }

        Entity.Quantity = value;
    }

    // Las categorias se piden antes de que el usuario elija producto
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ProductCategory>>("api/v1/productcategories/loadCombo");
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

    // El traslado padre de una linea nueva
    public void SetTransferId(Guid transferId)
    {
        Entity.TransferId = transferId;
    }

    public async Task ChangeCategoryAsync(Guid productCategoryId)
    {
        ProductCategoryId = productCategoryId;

        if (IsInitializingForm)
        {
            await LoadProductsAsync(productCategoryId);
            return;
        }

        Entity.ProductId = Guid.Empty;
        StockAvailable = 0;
        Products = new ObservableCollection<ProductEntity>();
        await LoadProductsAsync(productCategoryId);
    }

    // Al elegir producto se pregunta cuanto hay en la bodega de origen de ESTE traslado
    public async Task ChangeProductAsync(Guid productId)
    {
        Entity.ProductId = productId;

        //Si el producto lleva serial hay que ELEGIR los equipos, no escribir una cantidad
        ProductWithSerials = Products.FirstOrDefault(x => x.ProductId == productId)?.WithSerials ?? false;
        AvailableSerials = new ObservableCollection<SerialPickRow>();

        if (ProductWithSerials)
        {
            await LoadSerialsAsync(productId);
        }

        if (productId == Guid.Empty || Entity.TransferId == Guid.Empty)
        {
            StockAvailable = 0;
            return;
        }

        var url = $"api/v1/productStocks/transferstock?TransferId={Entity.TransferId}&ProductId={productId}";
        var response = await _repository.GetAsync<TransferStockDTO>(url);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            StockAvailable = 0;
            return;
        }

        StockAvailable = response.Response?.DiponibleOrigen ?? 0;

        if (Quantity > StockAvailable)
        {
            Quantity = StockAvailable;
        }
    }

    // La linea existente, con su categoria y su lista de productos ya puestas
    public async Task LoadForEditAsync(Guid id)
    {
        IsInitializingForm = true;

        try
        {
            await LoadAsync(id);

            Quantity = Entity.Quantity;

            if (Entity.Product is not null)
            {
                ProductCategoryId = Entity.Product.ProductCategoryId;
                await LoadProductsAsync(ProductCategoryId);
            }

            await ChangeProductAsync(Entity.ProductId);
        }
        finally
        {
            IsInitializingForm = false;
        }
    }

    //Guarda la linea y DESPUES sus equipos. No se usa el SaveChangesAsync del padre
    //porque al crear hace falta el id que devuelve el servidor para reservarlos.
    protected async Task GuardarConSerialesAsync(bool isEdit)
    {
        var validacion = GetValidationMessage();
        if (!string.IsNullOrWhiteSpace(validacion))
        {
            await _alertService.WarningAsync("Campo requerido", validacion);
            return;
        }

        IsSaving = true;

        try
        {
            var response = isEdit
                ? await _repository.PutAsync<TransferDetails, TransferDetails>(BaseUrl, Entity)
                : await _repository.PostAsync<TransferDetails, TransferDetails>(BaseUrl, Entity);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var id = isEdit ? Entity.TransferDetailsId : response.Response?.TransferDetailsId ?? Guid.Empty;

            if (id != Guid.Empty && !await SaveSerialsAsync(id))
            {
                //La linea quedo guardada: se cierra igual para que el usuario vea el estado real
                await _modalService.CloseAsync(ModalResult.Ok());
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    //Los equipos que se pueden mover: disponibles en la bodega de ORIGEN y sin reservar
    private async Task LoadSerialsAsync(Guid productId)
    {
        var url = $"api/v2/transferDetails/serials/available?transferId={Entity.TransferId}&productId={productId}";

        if (Entity.TransferDetailsId != Guid.Empty)
        {
            url += $"&transferDetailsId={Entity.TransferDetailsId}";
        }

        var response = await _repository.GetAsync<List<GuidItemModel>>(url);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var filas = (response.Response ?? new List<GuidItemModel>())
            .Select(x => new SerialPickRow(x, Recontar))
            .ToList();

        //Al editar se marcan los que la linea ya tenia
        if (Entity.TransferDetailsId != Guid.Empty)
        {
            var yaTiene = await _repository.GetAsync<List<GuidItemModel>>(
                $"api/v2/transferDetails/serials/line/{Entity.TransferDetailsId}");

            if (!await _responseHandler.HandleErrorAsync(yaTiene))
            {
                var ids = (yaTiene.Response ?? new List<GuidItemModel>()).Select(x => x.Value).ToHashSet();
                foreach (var fila in filas.Where(x => ids.Contains(x.Id)))
                {
                    fila.MarcarSinAvisar(true);
                }
            }
        }

        AvailableSerials = new ObservableCollection<SerialPickRow>(filas);
        Recontar();
    }

    //La cantidad sigue a cuantos equipos quedaron marcados
    private void Recontar()
    {
        if (!ProductWithSerials)
        {
            return;
        }

        Quantity = AvailableSerials.Count(x => x.IsSelected);
    }

    //Los equipos elegidos se guardan DESPUES de la linea, porque hasta ese momento no
    //existe a cual reservarlos. Devuelve false si el backend los rechazo.
    public async Task<bool> SaveSerialsAsync(Guid transferDetailsId)
    {
        if (!ProductWithSerials)
        {
            return true;
        }

        var elegidos = AvailableSerials.Where(x => x.IsSelected).Select(x => x.Id).ToList();

        var response = await _repository.PostAsync($"api/v2/transferDetails/serials/{transferDetailsId}", elegidos);

        return !await _responseHandler.HandleErrorAsync(response);
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

public partial class CreateTransferDetailDialogViewModel : TransferDetailFormViewModel
{
    public CreateTransferDetailDialogViewModel(
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
        await GuardarConSerialesAsync(false);
    }
}

public partial class EditTransferDetailDialogViewModel : TransferDetailFormViewModel
{
    public EditTransferDetailDialogViewModel(
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
        await GuardarConSerialesAsync(true);
    }
}

// Un equipo de la lista, con su casilla. Avisa al formulario para que la cantidad
// siga a cuantos quedaron marcados.
public partial class SerialPickRow : ObservableObject
{
    private readonly Action _alMarcar;
    private bool _avisar = true;

    [ObservableProperty]
    private bool _isSelected;

    public Guid Id { get; }

    public string MacWlan { get; }

    public SerialPickRow(GuidItemModel item, Action alMarcar)
    {
        Id = item.Value;
        MacWlan = item.Name ?? string.Empty;
        _alMarcar = alMarcar;
    }

    //Para marcar los que ya tenia la linea sin disparar el recuento una vez por fila
    public void MarcarSinAvisar(bool marcado)
    {
        _avisar = false;
        IsSelected = marcado;
        _avisar = true;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (_avisar)
        {
            _alMarcar();
        }
    }
}
