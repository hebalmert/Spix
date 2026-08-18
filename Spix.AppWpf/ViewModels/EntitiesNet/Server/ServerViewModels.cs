using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.NetHelper;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesNet.Server;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.MkDTOs;
using Spix.HttpService;
using System.Collections.ObjectModel;
using IpNetworkEntity = Spix.Domain.EntitiesNet.IpNetwork;
using ServerEntity = Spix.Domain.EntitiesNet.Server;

namespace Spix.AppWpf.ViewModels.EntitiesNet.Server;

// Lista los servidores y coordina sus operaciones de red locales desde WPF.
public partial class ServerIndexViewModel : PagedListViewModel<ServerEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/servers";

    public ServerIndexViewModel(
        IPagedEntityService<ServerEntity> pagedEntityService,
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
        ModalResult result = await _modalService.ShowAsync<CreateServerDialogView>("Crear servidor");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El servidor fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ServerEntity? server)
    {
        if (server == null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = server.ServerId
        };

        ModalResult result = await _modalService.ShowAsync<EditServerDialogView>("Editar servidor", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El servidor fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ServerEntity? server)
    {
        if (server == null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar servidor",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{server.ServerId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El servidor fue eliminado correctamente.");
    }

    // Ejecuta ICMP desde el computador Windows hacia la IP configurada en el servidor.
    [RelayCommand]
    private async Task PingAsync(ServerEntity? server)
    {
        string? host = server?.IpNetwork?.Ip;
        if (string.IsNullOrWhiteSpace(host))
        {
            await _alertService.WarningAsync("Ping", "El servidor no tiene una IP de red disponible para ejecutar el ping.");
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Host"] = host,
            ["ServerName"] = server!.ServerName
        };

        await _modalService.ShowAsync<ServerPingDialogView>("Ping del servidor", parameters);
    }

    // Entrega el servidor del indice a la prueba MikroTik ejecutada desde Windows.
    [RelayCommand]
    private async Task CheckMikrotikAsync(ServerEntity? server)
    {
        if (server == null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Server"] = server
        };

        await _modalService.ShowAsync<ServerMikrotikDialogView>("Conexion MikroTik", parameters);
    }
}

// Comparte los selects y las validaciones del formulario de servidores.
public abstract partial class ServerFormViewModel : CrudFormViewModel<ServerEntity>
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
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isInitializing;

    protected override string BaseUrl => "api/v1/servers";

    protected ServerFormViewModel(
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

    protected override ServerEntity CreateEntity()
    {
        return new ServerEntity
        {
            Active = true,
            ApiPort = 8728
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.ServerName))
        {
            return "Debes ingresar el nombre del servidor.";
        }

        if (Entity.IpNetworkId == Guid.Empty)
        {
            return "Debes seleccionar la IP de red.";
        }

        if (Entity.MarkId == Guid.Empty || Entity.MarkModelId == Guid.Empty)
        {
            return "Debes seleccionar la marca y el modelo.";
        }

        if (string.IsNullOrWhiteSpace(Entity.Usuario) || string.IsNullOrWhiteSpace(Entity.Clave))
        {
            return "Debes ingresar el usuario y la clave MikroTik.";
        }

        if (string.IsNullOrWhiteSpace(Entity.WanName))
        {
            return "Debes ingresar el nombre de la interfaz WAN.";
        }

        if (Entity.ApiPort <= 0)
        {
            return "Debes ingresar un puerto API valido.";
        }

        if (Entity.ZoneId == Guid.Empty)
        {
            return "Debes seleccionar la zona.";
        }

        return null;
    }

    // Carga los datos independientes y luego los selects que dependen de la entidad actual.
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

    // Restablece ciudad y zona cuando cambia el estado.
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

    // Restablece la zona para que corresponda a la ciudad seleccionada.
    public async Task ChangeCityAsync(int cityId)
    {
        Entity.CityId = cityId;
        Entity.ZoneId = Guid.Empty;
        Zones = new ObservableCollection<Zone>();
        OnPropertyChanged(nameof(Entity));
        await LoadZonesAsync(cityId);
    }

    // Carga solo los modelos pertenecientes a la marca seleccionada.
    public async Task ChangeMarkAsync(Guid markId)
    {
        Entity.MarkId = markId;
        Entity.MarkModelId = Guid.Empty;
        MarkModels = new ObservableCollection<MarkModel>();
        OnPropertyChanged(nameof(Entity));
        await LoadMarkModelsAsync(markId);
    }

    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    // Obtiene la entidad antes de reconstruir sus listas dependientes para editarla.
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
}

public partial class CreateServerDialogViewModel : ServerFormViewModel
{
    public CreateServerDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditServerDialogViewModel : ServerFormViewModel
{
    public EditServerDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}

// Ejecuta ping local contra un servidor sin delegar la comunicacion al Backend.
public partial class ServerPingDialogViewModel : ObservableObject
{
    private readonly IPingControl _pingControl;
    private readonly ModalService _modalService;

    [ObservableProperty]
    private string _host = string.Empty;

    [ObservableProperty]
    private string _serverName = string.Empty;

    [ObservableProperty]
    private PingResult? _result;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ServerPingDialogViewModel(IPingControl pingControl, ModalService modalService)
    {
        _pingControl = pingControl;
        _modalService = modalService;
    }

    public void SetHost(string host, string? serverName)
    {
        Host = host;
        ServerName = string.IsNullOrWhiteSpace(serverName) ? host : serverName;
        Result = null;
        ErrorMessage = string.Empty;
    }

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
            if (!response.WasSuccess)
            {
                ErrorMessage = response.Message ?? "El servidor no respondio al ping.";
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

// Prueba MikroTik desde Windows con la configuracion completa que ya entrega el indice.
public partial class ServerMikrotikDialogViewModel : ObservableObject
{
    private readonly IMkConnectionControl _mkConnectionControl;
    private readonly ModalService _modalService;
    private ServerEntity? _serverToCheck;

    [ObservableProperty]
    private ServerEntity? _server;

    [ObservableProperty]
    private MkConnectionResultDTO? _result;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ConnectionTitle => HasError
        ? "No fue posible conectar con MikroTik"
        : Result?.Text ?? "Comprobando conexion...";

    public ServerMikrotikDialogViewModel(
        IMkConnectionControl mkConnectionControl,
        ModalService modalService)
    {
        _mkConnectionControl = mkConnectionControl;
        _modalService = modalService;
    }

    // Actualiza la apariencia del estado cuando la conexion informa un error.
    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(ConnectionTitle));
    }

    // Actualiza el titulo cuando la conexion completa exitosamente.
    partial void OnResultChanged(MkConnectionResultDTO? value)
    {
        OnPropertyChanged(nameof(ConnectionTitle));
    }

    public async Task InitializeAsync(ServerEntity? server)
    {
        if (server == null)
        {
            ErrorMessage = "No fue posible identificar el servidor.";
            return;
        }

        _serverToCheck = server;
        IsLoading = true;
        ErrorMessage = string.Empty;
        Result = null;

        try
        {
            Server = server;
            var connectionResponse = await _mkConnectionControl.CheckConnectionAsync(server);
            Result = connectionResponse.Result;
            if (!connectionResponse.WasSuccess)
            {
                ErrorMessage = connectionResponse.Message ?? "No fue posible conectar con MikroTik.";
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
    private async Task RetryAsync()
    {
        if (_serverToCheck != null)
        {
            await InitializeAsync(_serverToCheck);
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
