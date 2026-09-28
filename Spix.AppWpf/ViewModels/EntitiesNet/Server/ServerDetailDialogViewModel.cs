using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesNet.Server;

public partial class ServerDetailDialogViewModel : ObservableObject
{
    private const string ServerUrl = "api/v1/servers";
    private const string LocalPppoeUrl = "api/v2/serverpppoelocal";
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;
    private Guid _serverId;
    private string _serverIp = string.Empty;
    private bool _saved;

    [ObservableProperty] private Spix.Domain.EntitiesNet.Server? _selected;
    [ObservableProperty] private ObservableCollection<IntItemModel> _controlTypes = new();
    [ObservableProperty] private ObservableCollection<MkInterfaceDTO> _interfaces = new();
    //Calificado completo: el namespace de este archivo es ...ViewModels.EntitiesNet.Server
    //y existe un namespace ...ViewModels.EntitiesNet.IpNet, asi que 'IpNet' a secas
    //resuelve al namespace y no al tipo. Lo mismo pasa arriba con Server.
    [ObservableProperty] private ObservableCollection<Spix.Domain.EntitiesNet.IpNetwork> _localIps = new();
    [ObservableProperty] private string _wanName = string.Empty;
    [ObservableProperty] private string _lanName = string.Empty;
    [ObservableProperty] private int _controlTypeValue;
    [ObservableProperty] private Guid? _localIpId;
    [ObservableProperty] private string _serviceName = string.Empty;
    [ObservableProperty] private string _connectionText = "Conexion sin probar";
    [ObservableProperty] private bool _isProvisioned;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isSaving;

    public bool IsPppoe => ControlTypeValue == (int)MikrotikControlType.PPPoE;
    public bool CanProvision => IsPppoe && !IsProvisioned;

    //El borrado solo tiene sentido al reves: ya creado
    public bool CanRemove => IsPppoe && IsProvisioned;

    partial void OnControlTypeValueChanged(int value)
    {
        OnPropertyChanged(nameof(IsPppoe));
        OnPropertyChanged(nameof(CanProvision));
        OnPropertyChanged(nameof(CanRemove));
    }

    partial void OnIsProvisionedChanged(bool value)
    {
        OnPropertyChanged(nameof(CanProvision));
        OnPropertyChanged(nameof(CanRemove));
    }

    public ServerDetailDialogViewModel(IRepository repository, HttpResponseHandler responseHandler,
        ModalService modalService, AlertService alertService, ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    public async Task InitializeAsync(Guid serverId, string? serverIp)
    {
        _serverId = serverId;
        _serverIp = serverIp ?? string.Empty;
        IsLoading = true;
        try
        {
            var response = await _repository.GetAsync<Spix.Domain.EntitiesNet.Server>($"{ServerUrl}/{serverId}");
            if (await _responseHandler.HandleErrorAsync(response)) return;

            Selected = response.Response;
            if (Selected == null) return;
            WanName = Selected.WanName ?? string.Empty;
            LanName = Selected.LanName ?? string.Empty;
            ControlTypeValue = (int)Selected.ControlMk;
            LocalIpId = Selected.PppLocalIpNetId;
            ServiceName = Selected.PppServiceName ?? string.Empty;
            IsProvisioned = !string.IsNullOrWhiteSpace(Selected.PppServerMkId);

            var typeResponse = await _repository.GetAsync<List<IntItemModel>>($"{ServerUrl}/controlTypes");
            if (!await _responseHandler.HandleErrorAsync(typeResponse))
            {
                ControlTypes = new ObservableCollection<IntItemModel>(typeResponse.Response ?? new());
            }

            //Lista propia de la IP local: el backend decide que se puede elegir
            //sabiendo de que servidor se trata (ver ComboLocalPppAsync).
            var ipResponse = await _repository.GetAsync<List<Spix.Domain.EntitiesNet.IpNetwork>>(
                $"api/v1/ipnetworks/loadComboLocalPpp/{Selected.ServerId}");
            if (!await _responseHandler.HandleErrorAsync(ipResponse))
            {
                LocalIps = new ObservableCollection<Spix.Domain.EntitiesNet.IpNetwork>(ipResponse.Response ?? new());
            }

            var names = new[] { WanName, LanName }.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct();
            Interfaces = new ObservableCollection<MkInterfaceDTO>(names.Select(x => new MkInterfaceDTO { Name = x, Text = x }));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckConnectionAsync()
    {
        if (Selected == null) return;
        IsLoading = true;
        ConnectionText = "Probando conexion local...";
        try
        {
            var found = new List<MkInterfaceDTO>();
            var result = await _mikrotikService.ExecuteAsync(LocalServer(), router =>
                found = LocalPppoeServerCommands.ReadInterfaces(router));
            if (!result.WasExecuted)
            {
                ConnectionText = result.Message;
                return;
            }

            found.Insert(0, new MkInterfaceDTO { Name = string.Empty, Text = "Seleccione interfaz" });
            Interfaces = new ObservableCollection<MkInterfaceDTO>(found);
            ConnectionText = $"Conexion local correcta. {found.Count - 1} interfaces disponibles.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (await SaveSelectedAsync())
        {
            await _alertService.SuccessAsync("Servidor", "Configuracion guardada.");
        }
    }

    private async Task<bool> SaveSelectedAsync()
    {
        if (Selected == null) return false;
        if (!Enum.IsDefined(typeof(MikrotikControlType), ControlTypeValue))
        {
            await _alertService.WarningAsync("Servidor", "Seleccione el tipo de control.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(WanName) && WanName == LanName)
        {
            await _alertService.WarningAsync("Servidor", "Las interfaces de internet y clientes deben ser distintas.");
            return false;
        }

        Selected.WanName = string.IsNullOrWhiteSpace(WanName) ? null : WanName;
        Selected.LanName = string.IsNullOrWhiteSpace(LanName) ? null : LanName;
        Selected.ControlMk = (MikrotikControlType)ControlTypeValue;
        Selected.PppLocalIpNetId = LocalIpId == Guid.Empty ? null : LocalIpId;

        IsSaving = true;
        try
        {
            var response = await _repository.PutAsync(ServerUrl, Selected);
            if (await _responseHandler.HandleErrorAsync(response)) return false;
            _saved = true;
            return true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ProvisionAsync()
    {
        if (!CanProvision || Selected == null) return;
        if (string.IsNullOrWhiteSpace(LanName) || !LocalIpId.HasValue || LocalIpId.Value == Guid.Empty)
        {
            await _alertService.WarningAsync("Servidor PPPoE", "Seleccione la interfaz de clientes y la IP local.");
            return;
        }

        if (!await SaveSelectedAsync()) return;

        var setupUrl = $"{LocalPppoeUrl}/{_serverId}";
        if (!string.IsNullOrWhiteSpace(ServiceName))
        {
            setupUrl += $"?serviceName={Uri.EscapeDataString(ServiceName)}";
        }

        var setupResponse = await _repository.GetAsync<PppoeServerLocalSetupDTO>(setupUrl);
        if (await _responseHandler.HandleErrorAsync(setupResponse)) return;
        var setup = setupResponse.Response;
        if (setup == null || setup.IsProvisioned)
        {
            await _alertService.WarningAsync("Servidor PPPoE", "El servidor ya esta configurado o faltan datos.");
            return;
        }

        IsSaving = true;
        try
        {
            (string ProfileId, string ServerId) created = default;
            var router = new Spix.Domain.EntitiesNet.Server
            {
                ServerId = setup.ServerId,
                ServerName = setup.ServerName,
                Usuario = setup.Username,
                Clave = setup.Password,
                ApiPort = setup.ApiPort,
                IpNetwork = new Spix.Domain.EntitiesNet.IpNetwork { Ip = setup.ServerIp }
            };

            var result = await _mikrotikService.ExecuteAsync(router,
                mk => created = LocalPppoeServerCommands.Provision(mk, setup),
                timeout: TimeSpan.FromMinutes(2));
            if (!result.WasExecuted)
            {
                await _alertService.ErrorAsync("Servidor PPPoE", result.Message);
                return;
            }

            var save = new PppoeServerLocalSaveDTO
            {
                ServerId = setup.ServerId,
                ProfileName = setup.ProfileName,
                ProfileMikrotikId = created.ProfileId,
                ServiceName = setup.ServiceName,
                ServerMikrotikId = created.ServerId
            };
            var response = await _repository.PostAsync<PppoeServerLocalSaveDTO, bool>(LocalPppoeUrl, save);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _alertService.WarningAsync("Servidor PPPoE",
                    "El MikroTik fue configurado, pero Spix no confirmo los IDs. Revise ambos antes de reintentar.");
                return;
            }

            IsProvisioned = true;
            _saved = true;
            await _alertService.SuccessAsync("Servidor PPPoE", "Perfil y servidor PPPoE configurados desde esta PC.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    //Borra la configuracion PPPoE DEL EQUIPO desde esta PC, para poder rehacerla.
    //Mismo reparto que el crear: el backend prepara y guarda, el MikroTik lo toca el
    //escritorio por la LAN. El backend se niega si hay contratos pegados al servidor.
    [RelayCommand]
    private async Task RemovePppoeAsync()
    {
        if (!CanRemove || Selected == null) return;

        if (!await _alertService.ConfirmAsync("Servidor PPPoE",
                "Se borraran el perfil y el servidor PPPoE del equipo. La configuracion se puede volver a crear.",
                "Eliminar"))
        {
            return;
        }

        var setupResponse = await _repository.GetAsync<PppoeServerLocalRemoveDTO>($"{LocalPppoeUrl}/remove/{_serverId}");
        if (await _responseHandler.HandleErrorAsync(setupResponse)) return;

        var datos = setupResponse.Response;
        if (datos == null) return;

        IsSaving = true;
        try
        {
            var router = new Spix.Domain.EntitiesNet.Server
            {
                ServerId = datos.ServerId,
                Usuario = datos.Username,
                Clave = datos.Password,
                ApiPort = datos.ApiPort,
                IpNetwork = new Spix.Domain.EntitiesNet.IpNetwork { Ip = datos.ServerIp }
            };

            var result = await _mikrotikService.ExecuteAsync(router,
                mk => LocalPppoeServerCommands.Remove(mk, datos.ProfileMikrotikId, datos.ServerMikrotikId),
                timeout: TimeSpan.FromMinutes(2));

            if (!result.WasExecuted)
            {
                await _alertService.ErrorAsync("Servidor PPPoE", result.Message);
                return;
            }

            //El equipo ya quedo limpio: ahora el espejo
            var response = await _repository.DeleteAsync($"{LocalPppoeUrl}/{_serverId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _alertService.WarningAsync("Servidor PPPoE",
                    "El MikroTik quedo limpio, pero Spix no pudo borrar su registro. Revise antes de reintentar.");
                return;
            }

            IsProvisioned = false;
            _saved = true;
            await _alertService.SuccessAsync("Servidor PPPoE", "Configuracion PPPoE borrada desde esta PC.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync() => await _modalService.CloseAsync(_saved ? ModalResult.Ok() : ModalResult.Cancel());

    private Spix.Domain.EntitiesNet.Server LocalServer() => new()
    {
        ServerId = _serverId,
        ServerName = Selected!.ServerName,
        Usuario = Selected.Usuario,
        Clave = Selected.Clave,
        ApiPort = Selected.ApiPort,
        IpNetwork = new Spix.Domain.EntitiesNet.IpNetwork { Ip = _serverIp }
    };
}
