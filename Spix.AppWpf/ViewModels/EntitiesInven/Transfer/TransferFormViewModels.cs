using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using TransferEntity = Spix.Domain.EntitiesInven.Transfer;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// El encabezado del traslado: de que bodega sale y a cual entra.
//
// Las dos listas son la MISMA consulta, igual que en la web: una bodega puede ser origen
// o destino. Lo unico que no se permite es que sean la misma, porque un traslado a si
// mismo no mueve nada y dejaria el inventario tocado sin razon.
public abstract partial class TransferFormViewModel : CrudFormViewModel<TransferEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<ProductStorage> _productStorages = new();

    //Quien recibe los equipos: tecnicos y usuarios en una sola lista. La arma el
    //Backend con su neutro; aqui solo se pinta.
    [ObservableProperty]
    private ObservableCollection<TextItemModel> _receivers = new();

    protected override string BaseUrl => "api/v2/transfers";

    protected TransferFormViewModel(
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

    protected override TransferEntity CreateEntity()
    {
        return new TransferEntity
        {
            DateTransfer = DateTime.UtcNow,
            Status = TransferType.Pendiente
        };
    }

    protected override string? GetValidationMessage()
    {
        if (Entity.FromProductStorageId == Guid.Empty)
        {
            return "Debes seleccionar la bodega de origen.";
        }

        if (Entity.ToProductStorageId == Guid.Empty)
        {
            return "Debes seleccionar la bodega de destino.";
        }

        if (Entity.FromProductStorageId == Entity.ToProductStorageId)
        {
            return "La bodega de origen y la de destino no pueden ser la misma.";
        }

        return null;
    }

    // Las bodegas se piden una sola vez y sirven para los dos selects
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ProductStorage>>("api/v1/combosData/ComboStorage");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            ProductStorages = new ObservableCollection<ProductStorage>(
                response.Response ?? new List<ProductStorage>());

            var recibe = await _repository.GetAsync<List<TextItemModel>>(
                "api/v2/transfers/loadComboReceivers");

            if (!await _responseHandler.HandleErrorAsync(recibe))
            {
                Receivers = new ObservableCollection<TextItemModel>(
                    recibe.Response ?? new List<TextItemModel>());
            }
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

    // El encabezado se carga DESPUES de tener las bodegas, o el select llega sin su lista
    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
    }
}

public partial class CreateTransferDialogViewModel : TransferFormViewModel
{
    public CreateTransferDialogViewModel(
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

public partial class EditTransferDialogViewModel : TransferFormViewModel
{
    public EditTransferDialogViewModel(
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
