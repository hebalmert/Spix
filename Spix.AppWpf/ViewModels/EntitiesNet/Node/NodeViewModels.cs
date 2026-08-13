using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesNet.Node;
using Spix.AppWpf.NetHelper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;
using IpNetworkEntity = Spix.Domain.EntitiesNet.IpNetwork;
using NodeEntity = Spix.Domain.EntitiesNet.Node;

namespace Spix.AppWpf.ViewModels.EntitiesNet.Node;

// Lista los nodos de acceso y conserva las acciones disponibles en el indice Blazor.
public partial class NodeIndexViewModel : PagedListViewModel<NodeEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/nodes";

    public NodeIndexViewModel(
        IPagedEntityService<NodeEntity> pagedEntityService,
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
        var result = await _modalService.ShowAsync<CreateNodeDialogView>("Crear nodo");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El nodo fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(NodeEntity? node)
    {
        if (node is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = node.NodeId
        };

        var result = await _modalService.ShowAsync<EditNodeDialogView>("Editar nodo", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El nodo fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(NodeEntity? node)
    {
        if (node is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar nodo",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{node.NodeId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El nodo fue eliminado correctamente.");
    }

    [RelayCommand]
    private async Task ViewMapAsync(NodeEntity? node)
    {
        if (node?.Latitude is null || node.Longitude is null)
        {
            await _alertService.WarningAsync("Mapa", "Este nodo no tiene coordenadas.");
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Latitude"] = node.Latitude.Value,
            ["Longitude"] = node.Longitude.Value,
            ["Title"] = node.NodesName
        };

        await _modalService.ShowAsync<NodeMapDialogView>("Mapa", parameters);
    }

    // Ejecuta el ping desde el Windows local contra la IP configurada en el nodo.
    [RelayCommand]
    private async Task PingAsync(NodeEntity? node)
    {
        string? host = node?.IpNetwork?.Ip;
        if (string.IsNullOrWhiteSpace(host))
        {
            await _alertService.WarningAsync(
                "Ping",
                "El nodo no tiene una IP de red disponible para ejecutar el ping.");
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Host"] = host,
            ["NodeName"] = node!.NodesName
        };

        await _modalService.ShowAsync<NodePingDialogView>("Ping del nodo", parameters);
    }
}

// Comparte los combos y las reglas propias del formulario de nodos de acceso.
public abstract partial class NodeFormViewModel : CrudFormViewModel<NodeEntity>
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
    private ObservableCollection<IntItemModel> _operations = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _channels = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _securities = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _frecuencyTypes = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _frecuencies = new();

    [ObservableProperty]
    private IntItemModel? _selectedFrecuency;

    [ObservableProperty]
    private string _coordinatesText = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isInitializing;

    protected override string BaseUrl => "api/v1/nodes";

    protected NodeFormViewModel(
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

    protected override NodeEntity CreateEntity()
    {
        return new NodeEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.NodesName))
        {
            return "Debes ingresar el SSID del nodo.";
        }

        if (Entity.OperationId <= 0)
        {
            return "Debes seleccionar la operacion.";
        }

        if (Entity.IpNetworkId == Guid.Empty)
        {
            return "Debes seleccionar la IP de red.";
        }

        if (string.IsNullOrWhiteSpace(Entity.Usuario) || string.IsNullOrWhiteSpace(Entity.Clave))
        {
            return "Debes ingresar el usuario y la clave del nodo.";
        }

        if (Entity.MarkId == Guid.Empty || Entity.MarkModelId == Guid.Empty)
        {
            return "Debes seleccionar la marca y el modelo.";
        }

        if (Entity.ZoneId == Guid.Empty)
        {
            return "Debes seleccionar la zona.";
        }

        return null;
    }

    // Carga todos los combos independientes antes de que el usuario pueda completar el nodo.
    public async Task InitializeAsync()
    {
        IsLoading = true;
        IsInitializing = true;

        try
        {
            await LoadStatesAsync();
            await LoadMarksAsync();
            await LoadIpNetworksAsync();
            await LoadOperationsAsync();
            await LoadChannelsAsync();
            await LoadSecuritiesAsync();
            await LoadFrecuencyTypesAsync();

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

    // Carga solamente las frecuencias posibles para el tipo escogido.
    public async Task ChangeFrecuencyTypeAsync(int frecuencyTypeId)
    {
        Entity.FrecuencyTypeId = frecuencyTypeId;
        Entity.FrecuencyId = 0;
        Frecuencies = new ObservableCollection<IntItemModel>();
        SelectedFrecuency = null;
        OnPropertyChanged(nameof(Entity));
        await LoadFrecuenciesAsync(frecuencyTypeId);
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

        if (Entity.FrecuencyTypeId is int frecuencyTypeId && frecuencyTypeId > 0)
        {
            await LoadFrecuenciesAsync(frecuencyTypeId);
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

    private async Task LoadOperationsAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>("api/v1/downData/OperationCombo");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Operations = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadChannelsAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>("api/v1/downData/channelCombo");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Channels = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadSecuritiesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>("api/v1/downData/SecurityCombo");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Securities = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadFrecuencyTypesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>("api/v1/downData/FreCuentyTypeCombo");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        FrecuencyTypes = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadFrecuenciesAsync(int frecuencyTypeId)
    {
        if (frecuencyTypeId <= 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<IntItemModel>>($"api/v1/downData/frecuency/{frecuencyTypeId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Frecuencies = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());

        SelectedFrecuency = Frecuencies.FirstOrDefault(item => item.Value == Entity.FrecuencyId)
            ?? Frecuencies.FirstOrDefault(item => item.Value == 0);

        Entity.FrecuencyId = SelectedFrecuency?.Value ?? 0;
        OnPropertyChanged(nameof(Entity));
    }

    // Mantiene la entidad sincronizada con el valor escogido en el select de frecuencia.
    partial void OnSelectedFrecuencyChanged(IntItemModel? value)
    {
        if (value == null)
        {
            return;
        }

        Entity.FrecuencyId = value.Value;
        OnPropertyChanged(nameof(Entity));
    }

    private void SetCoordinatesText()
    {
        if (Entity.Latitude.HasValue && Entity.Longitude.HasValue)
        {
            CoordinatesText = $"{Entity.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, {Entity.Longitude.Value.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}

public partial class CreateNodeDialogViewModel : NodeFormViewModel
{
    public CreateNodeDialogViewModel(
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

public partial class EditNodeDialogViewModel : NodeFormViewModel
{
    public EditNodeDialogViewModel(
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

// Conserva el punto seleccionado para el visor de mapas embebido de WPF.
public partial class NodeMapDialogViewModel : ObservableObject
{
    private readonly ModalService _modalService;

    [ObservableProperty]
    private decimal _latitude;

    [ObservableProperty]
    private decimal _longitude;

    [ObservableProperty]
    private string _title = "Mapa";

    public NodeMapDialogViewModel(ModalService modalService)
    {
        _modalService = modalService;
    }

    public void SetMap(decimal latitude, decimal longitude, string? title)
    {
        Latitude = latitude;
        Longitude = longitude;
        Title = string.IsNullOrWhiteSpace(title) ? "Mapa" : title;
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Resume un intento individual para presentar los tiempos locales dentro del modal de ping.
public class NodePingAttempt
{
    public int Number { get; init; }
    public long Time { get; init; }
    public bool WasSuccessful => Time >= 0;
    public string Text => WasSuccessful ? $"{Time} ms" : "Timeout";
}

// Ejecuta y presenta el ping local sin enviar la solicitud al Backend.
public partial class NodePingDialogViewModel : ObservableObject
{
    private readonly IPingControl _pingControl;
    private readonly ModalService _modalService;

    [ObservableProperty]
    private string _host = string.Empty;

    [ObservableProperty]
    private string _nodeName = string.Empty;

    [ObservableProperty]
    private PingResult? _result;

    [ObservableProperty]
    private ObservableCollection<NodePingAttempt> _attempts = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public NodePingDialogViewModel(
        IPingControl pingControl,
        ModalService modalService)
    {
        _pingControl = pingControl;
        _modalService = modalService;
    }

    public void SetHost(string host, string? nodeName)
    {
        Host = host;
        NodeName = string.IsNullOrWhiteSpace(nodeName) ? host : nodeName;
        Result = null;
        Attempts = new ObservableCollection<NodePingAttempt>();
        ErrorMessage = string.Empty;
    }

    // Ejecuta los cuatro intentos y conserva las metricas aunque el host no responda.
    public async Task InitializeAsync()
    {
        await RunPingAsync();
    }

    [RelayCommand]
    private async Task RunPingAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _pingControl.PingAsync(Host);
            Result = response.Result;

            Attempts = new ObservableCollection<NodePingAttempt>(
                Result?.Times.Select((time, index) => new NodePingAttempt
                {
                    Number = index + 1,
                    Time = time
                }) ?? Enumerable.Empty<NodePingAttempt>());

            if (Result is null)
            {
                ErrorMessage = response.Message ?? "No fue posible obtener el resultado del ping.";
                return;
            }

            if (!response.WasSuccess && string.IsNullOrWhiteSpace(Result.Message))
            {
                ErrorMessage = response.Message ?? "El nodo no respondio al ping.";
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
