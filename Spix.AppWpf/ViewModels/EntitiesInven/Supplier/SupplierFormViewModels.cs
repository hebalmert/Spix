using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.Collections.ObjectModel;
using SupplierEntity = Spix.Domain.EntitiesInven.Supplier;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Supplier;

// Carga los selects dependientes que usa el formulario de proveedores de Blazor.
public abstract partial class SupplierFormViewModel : CrudFormViewModel<SupplierEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<DocumentType> _documentTypes = new();

    [ObservableProperty]
    private ObservableCollection<State> _states = new();

    [ObservableProperty]
    private ObservableCollection<City> _cities = new();

    protected override string BaseUrl => "api/v1/suppliers";

    protected SupplierFormViewModel(
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

    protected override SupplierEntity CreateEntity()
    {
        return new SupplierEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.Name)) return "Debes ingresar el proveedor.";
        if (Entity.DocumentTypeId == Guid.Empty) return "Debes seleccionar el tipo de documento.";
        if (string.IsNullOrWhiteSpace(Entity.Document)) return "Debes ingresar el documento.";
        if (Entity.StateId <= 0 || Entity.CityId <= 0) return "Debes seleccionar el estado y la ciudad.";
        if (string.IsNullOrWhiteSpace(Entity.Email)) return "Debes ingresar el correo.";
        return null;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var documentResponse = await _repository.GetAsync<List<DocumentType>>("api/v1/combosData/ComboDocumentType");
            if (await _responseHandler.HandleErrorAsync(documentResponse)) return;
            DocumentTypes = new ObservableCollection<DocumentType>(documentResponse.Response ?? new List<DocumentType>());

            var stateResponse = await _repository.GetAsync<List<State>>("api/v1/combosData/ComboState");
            if (await _responseHandler.HandleErrorAsync(stateResponse)) return;
            States = new ObservableCollection<State>(stateResponse.Response ?? new List<State>());
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

    public async Task ChangeStateAsync(int stateId)
    {
        Entity.StateId = stateId;
        Entity.CityId = 0;
        Cities = new ObservableCollection<City>();
        OnPropertyChanged(nameof(Entity));
        await LoadCitiesAsync(stateId);
    }

    protected async Task LoadCitiesAsync(int stateId)
    {
        if (stateId <= 0) return;
        var response = await _repository.GetAsync<List<City>>($"api/v1/combosData/ComboCity/{stateId}");
        if (await _responseHandler.HandleErrorAsync(response)) return;
        Cities = new ObservableCollection<City>(response.Response ?? new List<City>());
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
        await LoadCitiesAsync(Entity.StateId);
    }
}

public partial class CreateSupplierDialogViewModel : SupplierFormViewModel
{
    public CreateSupplierDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

public partial class EditSupplierDialogViewModel : SupplierFormViewModel
{
    public EditSupplierDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);
}
