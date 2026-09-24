using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.Entities;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ZoneEntity = Spix.Domain.EntitiesGen.Zone;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Zone;

// Los campos de la zona: estado, ciudad, nombre y activo, con la ciudad colgando del
// estado igual que en Blazor.
public abstract partial class ZoneFormViewModel : CrudFormViewModel<ZoneEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<State> _states = new();

    [ObservableProperty]
    private ObservableCollection<City> _cities = new();

    //Mientras se esta cargando el formulario, el combo de estado dispara su evento de
    //cambio solo por seleccionar el valor que ya tenia la zona. Si no se ignora, ese
    //evento borra la ciudad justo antes de mostrarla.
    private bool _cargando;

    protected override string BaseUrl => "api/v1/zones";

    protected ZoneFormViewModel(
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

    protected override ZoneEntity CreateEntity()
    {
        return new ZoneEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (Entity.StateId <= 0 || Entity.CityId <= 0)
        {
            return "Debes seleccionar el estado y la ciudad.";
        }

        if (string.IsNullOrWhiteSpace(Entity.ZoneName))
        {
            return "Debes ingresar el nombre de la zona.";
        }

        return null;
    }

    // Los estados se piden una sola vez, al abrir el formulario
    public async Task InitializeAsync()
    {
        IsLoading = true;
        _cargando = true;

        try
        {
            var response = await _repository.GetAsync<List<State>>("api/v1/combosData/ComboState");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            States = new ObservableCollection<State>(response.Response ?? new List<State>());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsLoading = false;
            _cargando = false;
        }
    }

    // Al cambiar de estado se limpia la ciudad y se piden las del estado nuevo.
    // Solo cuando lo cambia el usuario: durante la carga no se toca nada.
    public async Task ChangeStateAsync(int stateId)
    {
        if (_cargando)
        {
            return;
        }

        Entity.StateId = stateId;
        Entity.CityId = 0;
        Cities = new ObservableCollection<City>();
        OnPropertyChanged(nameof(Entity));

        await LoadCitiesAsync(stateId);
    }

    private async Task LoadCitiesAsync(int stateId)
    {
        if (stateId <= 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<City>>($"api/v1/combosData/ComboCity/{stateId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Cities = new ObservableCollection<City>(response.Response ?? new List<City>());
    }

    // Al editar hay que traer tambien las ciudades del estado que ya tenia la zona
    public async Task LoadForEditAsync(Guid id)
    {
        _cargando = true;

        try
        {
            await LoadAsync(id);
            await LoadCitiesAsync(Entity.StateId);

            //La lista de ciudades llega despues que la zona: hay que avisarle al combo
            //para que vuelva a buscar cual le toca marcar
            OnPropertyChanged(nameof(Entity));
        }
        finally
        {
            _cargando = false;
        }
    }
}

public partial class CreateZoneDialogViewModel : ZoneFormViewModel
{
    public CreateZoneDialogViewModel(
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

public partial class EditZoneDialogViewModel : ZoneFormViewModel
{
    public EditZoneDialogViewModel(
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
