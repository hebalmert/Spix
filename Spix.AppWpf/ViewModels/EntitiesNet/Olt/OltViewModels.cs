using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesNet.Node;
using Spix.AppWpf.Views.EntitiesNet.Olt;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;
using IpNetworkEntity = Spix.Domain.EntitiesNet.IpNetwork;
using OltEntity = Spix.Domain.EntitiesNet.Olt;

namespace Spix.AppWpf.ViewModels.EntitiesNet.Olt;

// Lista las OLT, el equipo de central de la fibra. Es el gemelo del listado de Nodos:
// mismo endpoint por corporacion, mismo tablero y las mismas acciones del indice Blazor.
// La OLT solo guarda datos; el escritorio no le habla al equipo.
public partial class OltIndexViewModel : PagedListViewModel<OltListItemDto>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/olts";

    //Los cuatro numeros de arriba: los cuenta la base sobre TODAS las OLT,
    //no sobre la pagina que se esta viendo
    [ObservableProperty]
    private NetSummaryDto? _summary;

    // Despues de cada carga se vuelven a pedir: crear o borrar un equipo los cambia.
    protected override async Task AfterLoadAsync()
    {
        var responseHttp = await _repository.GetAsync<NetSummaryDto>("api/v1/olts/summary");

        if (responseHttp.Error)
        {
            //El tablero es informativo: si no llega, el listado sigue funcionando
            return;
        }

        Summary = responseHttp.Response;
    }

    public OltIndexViewModel(
        IPagedEntityService<OltListItemDto> pagedEntityService,
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
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateOltDialogView>("Crear OLT");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La OLT fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(OltListItemDto? olt)
    {
        if (olt is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = olt.OltId
        };

        var result = await _modalService.ShowAsync<EditOltDialogView>("Editar OLT", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La OLT fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(OltListItemDto? olt)
    {
        if (olt is null)
        {
            return;
        }

        //Con clientes colgados no se borra: el backend lo rechaza y aqui se avisa antes
        if (olt.Clients > 0)
        {
            await _alertService.WarningAsync(
                "Eliminar OLT",
                "Esta OLT tiene contratos asignados. Inactivela en vez de borrarla.");
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar OLT",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{olt.OltId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La OLT fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task ViewMapAsync(OltListItemDto? olt)
    {
        if (olt?.Latitude is null || olt.Longitude is null)
        {
            await _alertService.WarningAsync("Mapa", "Esta OLT no tiene coordenadas.");
            return;
        }

        //El visor de mapa es el mismo del nodo: recibe el punto y el titulo, nada propio del nodo
        var parameters = new Dictionary<string, object>
        {
            ["Latitude"] = olt.Latitude.Value,
            ["Longitude"] = olt.Longitude.Value,
            ["Title"] = olt.OltName
        };

        await _modalService.ShowAsync<NodeMapDialogView>("Mapa", parameters);
    }
}

// Comparte los combos y las reglas propias del formulario de OLT.
// Es el de Nodos sin lo inalambrico: no hay operacion, frecuencia, canal ni seguridad.
// En su lugar lleva cuantos puertos PON tiene y a que velocidad va cada uno.
public abstract partial class OltFormViewModel : CrudFormViewModel<OltEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<State> _states = new();

    [ObservableProperty]
    private ObservableCollection<City> _cities = new();

    [ObservableProperty]
    private ObservableCollection<Zone> _zones = new();

    [ObservableProperty]
    private ObservableCollection<Mark> _marks = new();

    [ObservableProperty]
    private ObservableCollection<MarkModel> _markModels = new();

    [ObservableProperty]
    private ObservableCollection<IpNetworkEntity> _ipNetworks = new();

    [ObservableProperty]
    private string _coordinatesText = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isInitializing;

    protected override string BaseUrl => "api/v1/olts";

    protected OltFormViewModel(
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

    protected override OltEntity CreateEntity()
    {
        return new OltEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.OltName))
        {
            return "Debes ingresar el nombre de la OLT.";
        }

        if (Entity.IpNetworkId == Guid.Empty)
        {
            return "Debes seleccionar la IP de red.";
        }

        if (string.IsNullOrWhiteSpace(Entity.Usuario))
        {
            return "Debes ingresar el usuario de la OLT.";
        }

        //Al editar la clave puede ir vacia: se conserva la guardada, igual que en Blazor
        if (!IsEdit && string.IsNullOrWhiteSpace(Entity.Clave))
        {
            return "Debes ingresar la clave de la OLT.";
        }

        if (Entity.MarkId == Guid.Empty || Entity.MarkModelId == Guid.Empty)
        {
            return "Debes seleccionar la marca y el modelo.";
        }

        if (Entity.ZoneId == Guid.Empty)
        {
            return "Debes seleccionar la zona.";
        }

        if (Entity.PortCount is < 1 or > 256)
        {
            return "Los puertos PON deben estar entre 1 y 256.";
        }

        return null;
    }

    //Lo sabe el hijo: crear exige la clave, editar la deja vacia para conservarla
    protected abstract bool IsEdit { get; }

    // Carga todos los combos independientes antes de que el usuario pueda completar la OLT.
    public async Task InitializeAsync()
    {
        IsLoading = true;
        IsInitializing = true;

        try
        {
            await LoadStatesAsync();
            await LoadMarksAsync();
            await LoadIpNetworksAsync();

            await LoadDependentCombosAsync();
            SetCoordinatesText();
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsInitializing = false;
            IsLoading = false;
        }
    }

    // Restablece los selects que dependen del estado cuando el usuario lo cambia.
    public async Task ChangeStateAsync(int stateId)
    {
        Entity.StateId = stateId;
        Entity.CityId = 0;
        Entity.ZoneId = Guid.Empty;
        Cities = new ObservableCollection<City>();
        Zones = new ObservableCollection<Zone>();
        OnPropertyChanged(nameof(Entity));
        await LoadCitiesAsync(stateId);
    }

    // Restablece las zonas para que siempre correspondan a la ciudad seleccionada.
    public async Task ChangeCityAsync(int cityId)
    {
        Entity.CityId = cityId;
        Entity.ZoneId = Guid.Empty;
        Zones = new ObservableCollection<Zone>();
        OnPropertyChanged(nameof(Entity));
        await LoadZonesAsync(cityId);
    }

    // Carga unicamente los modelos compatibles con la marca seleccionada.
    public async Task ChangeMarkAsync(Guid markId)
    {
        Entity.MarkId = markId;
        Entity.MarkModelId = Guid.Empty;
        MarkModels = new ObservableCollection<MarkModel>();
        OnPropertyChanged(nameof(Entity));
        await LoadMarkModelsAsync(markId);
    }

    // Convierte el texto de coordenadas al par decimal usado por la entidad de dominio.
    public async Task UpdateCoordinatesAsync(string coordinates)
    {
        CoordinatesText = coordinates;
        var parts = coordinates.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2 ||
            !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal latitude) ||
            !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal longitude))
        {
            await _alertService.WarningAsync(
                "Coordenadas",
                "Formato invalido. Use: 25.82370270482433, -80.38556718743175");
            return;
        }

        try
        {
            Entity.Latitude = latitude;
            Entity.Longitude = longitude;
            OnPropertyChanged(nameof(Entity));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            await _alertService.WarningAsync("Coordenadas", exception.Message);
        }
    }

    // Alterna la visualizacion de la clave sin modificar el valor que sera guardado.
    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    // Carga la entidad y despues sus dependencias, igual que la edicion en Blazor.
    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
        await InitializeAsync();
    }

    protected void SetCoordinatesText()
    {
        CoordinatesText = Entity.Latitude.HasValue && Entity.Longitude.HasValue
            ? $"{Entity.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, {Entity.Longitude.Value.ToString(CultureInfo.InvariantCulture)}"
            : string.Empty;
    }

    private async Task LoadDependentCombosAsync()
    {
        if (Entity.StateId > 0)
        {
            await LoadCitiesAsync(Entity.StateId);
        }

        if (Entity.CityId > 0)
        {
            await LoadZonesAsync(Entity.CityId);
        }

        if (Entity.MarkId != Guid.Empty)
        {
            await LoadMarkModelsAsync(Entity.MarkId);
        }
    }

    private async Task LoadStatesAsync()
    {
        var response = await _repository.GetAsync<List<State>>("api/v1/combosData/ComboState");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        States = new ObservableCollection<State>(response.Response ?? new List<State>());
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

    private async Task LoadZonesAsync(int cityId)
    {
        if (cityId <= 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<Zone>>($"api/v1/zones/loadCombo/{cityId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Zones = new ObservableCollection<Zone>(response.Response ?? new List<Zone>());
    }

    private async Task LoadMarksAsync()
    {
        var response = await _repository.GetAsync<List<Mark>>("api/v1/marks/loadCombo");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Marks = new ObservableCollection<Mark>(response.Response ?? new List<Mark>());
    }

    private async Task LoadMarkModelsAsync(Guid markId)
    {
        if (markId == Guid.Empty)
        {
            return;
        }

        var response = await _repository.GetAsync<List<MarkModel>>($"api/v1/marksmodels/loadCombo/{markId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        MarkModels = new ObservableCollection<MarkModel>(response.Response ?? new List<MarkModel>());
    }

    private async Task LoadIpNetworksAsync()
    {
        //Al editar se pide con el id para que la suya venga en la lista aunque este tomada
        string endpoint = Entity.IpNetworkId == Guid.Empty
            ? "api/v1/ipnetworks/loadCombo"
            : $"api/v1/ipnetworks/loadCombo/{Entity.IpNetworkId}";

        var response = await _repository.GetAsync<List<IpNetworkEntity>>(endpoint);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        IpNetworks = new ObservableCollection<IpNetworkEntity>(response.Response ?? new List<IpNetworkEntity>());
    }
}

public partial class CreateOltDialogViewModel : OltFormViewModel
{
    public CreateOltDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override bool IsEdit => false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditOltDialogViewModel : OltFormViewModel
{
    public EditOltDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override bool IsEdit => true;

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
