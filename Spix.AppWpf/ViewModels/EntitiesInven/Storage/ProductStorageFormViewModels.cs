using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Storage;

// Carga estados y ciudades para crear o editar bodegas con el mismo flujo de Blazor.
public abstract partial class ProductStorageFormViewModel : CrudFormViewModel<ProductStorage>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<State> _states = new();

    [ObservableProperty]
    private ObservableCollection<City> _cities = new();

    protected override string BaseUrl => "api/v1/productstorages";

    protected ProductStorageFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    protected override ProductStorage CreateEntity()
    {
        return new ProductStorage
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.StorageName)) return "Debes ingresar el nombre de la bodega.";
        if (Entity.StateId <= 0 || Entity.CityId <= 0) return "Debes seleccionar el estado y la ciudad.";
        return null;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            var response = await _repository.GetAsync<List<State>>("api/v1/combosData/ComboState");
            if (await _responseHandler.HandleErrorAsync(response)) return;
            States = new ObservableCollection<State>(response.Response ?? new List<State>());
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

    private async Task LoadCitiesAsync(int stateId)
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

public partial class CreateProductStorageDialogViewModel : ProductStorageFormViewModel
{
    public CreateProductStorageDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

public partial class EditProductStorageDialogViewModel : ProductStorageFormViewModel
{
    public EditProductStorageDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);
}
