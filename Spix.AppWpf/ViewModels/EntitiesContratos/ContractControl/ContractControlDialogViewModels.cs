using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

// El plan del cliente: dos listas en cascada, primero la categoria y luego el plan.
public partial class ContractPlanDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractplans";
    private const string CategoriesUrl = "api/v1/plancategories/loadCombo";
    private const string PlansUrl = "api/v1/plans/loadComboByCategory";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<PlanCategory> _categories = new();

    [ObservableProperty]
    private ObservableCollection<Plan> _plans = new();

    [ObservableProperty]
    private Guid _selectedCategoryId;

    [ObservableProperty]
    private Guid _selectedPlanId;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _contractClientId;

    public ContractPlanDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync(Guid contractClientId)
    {
        _contractClientId = contractClientId;

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<PlanCategory>>(CategoriesUrl);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Categories = new ObservableCollection<PlanCategory>(response.Response ?? new List<PlanCategory>());
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Al elegir la categoria se bajan SUS planes: el de antes ya no vale
    public async Task ChangeCategoryAsync(Guid categoryId)
    {
        SelectedCategoryId = categoryId;
        SelectedPlanId = Guid.Empty;

        Plans = new ObservableCollection<Plan>();

        if (categoryId == Guid.Empty)
        {
            return;
        }

        var response = await _repository.GetAsync<List<Plan>>($"{PlansUrl}/{categoryId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Plans = new ObservableCollection<Plan>(response.Response ?? new List<Plan>());
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedPlanId == Guid.Empty)
        {
            await _alertService.WarningAsync("Plan del cliente", "Debe elegir la categoria y el plan.");
            return;
        }

        IsSaving = true;

        try
        {
            var modelo = new ContractPlan
            {
                ContractClientId = _contractClientId,
                PlanId = SelectedPlanId
            };

            var response = await _repository.PostAsync(BaseUrl, modelo);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// La Queue de velocidad. No se elige nada: se arma con lo que YA quedo configurado
// —servidor, IP y plan— y el ESCRITORIO la escribe el mismo en el MikroTik.
//
// OJO, aqui esta la diferencia con la web: el Backend le habla al equipo por IP publica y
// el escritorio le habla por la red LAN. Por eso la porcion de MikroTik esta replicada
// aqui adentro, palabra por palabra, en vez de mandarla al Backend. Asi un cliente que no
// tenga IP publica se puede administrar igual.
//
// El API v2 solo hace dos cosas: entregar los datos —que servidor, que velocidades, si el
// queue padre ya existe y que IPs cuelgan de el— y, cuando el equipo ya quedo escrito,
// guardar el registro.
public partial class ContractQueueDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v2/contractmksetup/que";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private ContractQue _entity = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _contractClientId;
    private ContractQueSetupDTO? _setup;

    public ContractQueueDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    // Lo que se ve sale de las piezas que el detalle ya tiene en la mano, asi la pantalla
    // no espera a nadie. Los datos del equipo se piden aparte.
    public async Task InitializeAsync(Guid contractClientId, ContractServer? server, ContractIp? ip, ContractPlan? plan)
    {
        _contractClientId = contractClientId;

        Entity = new ContractQue
        {
            ContractClientId = contractClientId,
            ServerId = server?.ServerId ?? Guid.Empty,
            IpNetId = ip?.IpNetId ?? Guid.Empty,
            PlanId = plan?.PlanId ?? Guid.Empty,
            ServerName = server?.Server?.ServerName,
            IpServer = server?.Server?.IpNetwork?.Ip,
            IpCliente = ip?.IpNet?.Ip,
            PlanName = plan?.Plan?.PlanName,
            TotalVelocidad = plan?.Plan?.VelocidadTotal
        };

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<ContractQueSetupDTO>($"{BaseUrl}/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            _setup = response.Response;

            //Si falta algo se dice ahora, no despues de que el usuario oprima Crear
            if (_setup is null || !_setup.CanCreate)
            {
                await _alertService.WarningAsync(
                    "Queue de velocidad",
                    _setup?.Blocked ?? "No fue posible obtener los datos del servidor.");

                await _modalService.CloseAsync(ModalResult.Cancel());
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_setup is null || !_setup.CanCreate)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var datos = _setup;

            //===== El calculo, el mismo que hace el Backend =====

            //El target del padre: los clientes que ya cuelgan de el, mas el que entra
            int cantQueueClients = datos.ClientIps.Count;
            string ListaClientsIP;

            if (cantQueueClients == 0)
            {
                ListaClientsIP = datos.IpCliente!;
            }
            else
            {
                ListaClientsIP = $"{string.Join(", ", datos.ClientIps)}, {datos.IpCliente}";
            }

            //Todo en kbps, que es como se trabaja el padre
            int tasaReuso = datos.TasaReuso;
            int UpSpeed = datos.SpeedUpKbps;
            int DownSpeed = datos.SpeedDownKbps;
            int UpSpeedLimitAt = UpSpeed / tasaReuso;
            int DownSpeedLimitAt = DownSpeed / tasaReuso;
            int UpSpeedFather = cantQueueClients + 1 < tasaReuso + 1 ? UpSpeed : UpSpeedLimitAt * (cantQueueClients + 1);
            int DownSpeedFather = cantQueueClients + 1 < tasaReuso + 1 ? DownSpeed : DownSpeedLimitAt * (cantQueueClients + 1);

            string nomQueues = $"{datos.PcqUp}/{datos.PcqDown}";
            string nomParent = $"Parent {datos.PlanName} 1 a {tasaReuso}";

            //===== Las ordenes al equipo, por la red LAN =====

            var server = new Server
            {
                ServerId = datos.ServerId,
                ServerName = datos.ServerName,
                Usuario = datos.Usuario,
                Clave = datos.Clave,
                ApiPort = datos.ApiPort,
                IpNetwork = new IpNetwork { Ip = datos.ServerIp }
            };

            //Lo que hay que reportarle al API despues de escribir el equipo
            string parentIndex = datos.ParentMikrotikId ?? string.Empty;
            string parentName = datos.ParentName ?? nomParent;
            string queueIndex = string.Empty;
            bool parentCreated = !datos.HasParent;

            var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
            {
                int total;
                int rest;
                string idmk;

                if (!datos.HasParent)
                {
                    //No hay padre: se crea y se sube al tope de la lista
                    mikrotik.Send("/queue/simple/add");
                    mikrotik.Send("=limit-at=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                    mikrotik.Send("=max-limit=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                    mikrotik.Send("=name=" + nomParent);
                    mikrotik.Send("=queue=" + nomQueues);
                    mikrotik.Send("=target=" + ListaClientsIP);
                    mikrotik.Send("=priority=" + "5/5");
                    mikrotik.Send("/queue/simple/print", true);

                    foreach (var item in mikrotik.Read())
                    {
                        idmk = item;
                        total = idmk.Length;
                        rest = total - 10;
                        parentIndex = idmk.Substring(10, rest);
                    }

                    mikrotik.Send("/queue/simple/move");
                    mikrotik.Send("=.id=" + parentIndex);
                    mikrotik.Send("=destination=1");
                    mikrotik.Send("/queue/simple/print", true);

                    int sum1 = 0;
                    foreach (var item in mikrotik.Read())
                    {
                        sum1 += 1;
                    }

                    parentName = nomParent;
                }
                else
                {
                    //Ya hay padre: entra un cliente mas, se le recalcula y se le cambia el target
                    mikrotik.Send("/queue/simple/set");
                    mikrotik.Send("=.id=" + datos.ParentMikrotikId);
                    mikrotik.Send("=limit-at=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                    mikrotik.Send("=max-limit=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                    mikrotik.Send("=target=" + ListaClientsIP);
                    mikrotik.Send("/queue/simple/print", true);

                    foreach (var item in mikrotik.Read())
                    {
                        idmk = item;
                        total = idmk.Length;
                        rest = total - 10;
                    }
                }

                //El hijo: la queue del cliente, colgada del padre
                mikrotik.Send("/queue/simple/add");
                mikrotik.Send("=limit-at=" + $"{UpSpeedLimitAt}k/{DownSpeedLimitAt}k");
                mikrotik.Send("=max-limit=" + $"{UpSpeed}k/{DownSpeed}k");
                mikrotik.Send("=name=" + datos.NombreCliente);
                mikrotik.Send("=target=" + datos.IpCliente);
                mikrotik.Send("=parent=" + parentName);
                mikrotik.Send("=priority=" + "5/5");
                mikrotik.Send("=queue=" + nomQueues);
                mikrotik.Send("/queue/simple/print", true);

                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                    queueIndex = idmk.Substring(10, rest);
                }

                mikrotik.Send("/queue/simple/move");
                mikrotik.Send("=.id=" + queueIndex);
                mikrotik.Send("=destination=1");
                mikrotik.Send("/queue/simple/print", true);

                int sum = 0;
                foreach (var item in mikrotik.Read())
                {
                    sum += 1;
                }
            });

            if (!resultado.WasExecuted)
            {
                await _alertService.ErrorAsync("Queue de velocidad", resultado.Message);
                return;
            }

            //===== El equipo ya quedo escrito: ahora se guarda el registro =====

            var guardar = new ContractQueSaveDTO
            {
                ContractClientId = _contractClientId,
                ServerId = datos.ServerId,
                IpNetId = datos.IpNetId,
                PlanId = datos.PlanId,
                ServerName = datos.ServerName,
                IpServer = datos.ServerIp,
                IpCliente = datos.IpCliente,
                PlanName = datos.PlanName,
                TotalVelocidad = datos.VelocidadTotal,
                MikrotikId = queueIndex,
                ParentCreated = parentCreated,
                ParentName = parentName,
                ParentMikrotikId = parentIndex,
                ParentUp = $"{UpSpeed}k",
                ParentDown = $"{DownSpeed}k"
            };

            var response = await _repository.PostAsync(BaseUrl, guardar);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// El IpBinding de acceso. Como la Queue, se arma con lo ya configurado; lo unico que se
// elige es el tipo de acceso del HotSpot.
//
// Y como la Queue, lo escribe el ESCRITORIO en el equipo por la red LAN: la porcion de
// MikroTik esta replicada aqui adentro. El API v2 entrega los datos y guarda el registro.
public partial class ContractBindDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v2/contractmksetup/bind";
    private const string SetupUrl = "api/v2/contractmksetup/bind";
    private const string ConnUrl = "api/v2/contractmksetup/conn";
    private const string TypesUrl = "api/v1/combosData/ComboHotspot";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private ContractBind _entity = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _types = new();

    [ObservableProperty]
    private string _saveText = "Crear el IpBinding";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    //Con que servidor hablar y como se llama el cliente: es el comentario que ve el equipo
    private string? _serverIp;
    private string? _serverName;
    private string? _usuario;
    private string? _clave;
    private int _apiPort;
    private Guid _serverId;
    private string? _nombreCliente;
    private string? _blocked;

    public ContractBindDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _mikrotikService = mikrotikService;
    }

    public async Task InitializeAsync(Guid contractClientId, ContractServer? server, ContractIp? ip, ContractMac? mac)
    {
        Entity = new ContractBind
        {
            ContractClientId = contractClientId,
            ServerId = server?.ServerId ?? Guid.Empty,
            IpNetId = ip?.IpNetId ?? Guid.Empty,
            CargueDetailId = mac?.CargueDetailId ?? Guid.Empty,
            ServerName = server?.Server?.ServerName,
            IpServer = server?.Server?.IpNetwork?.Ip,
            IpCliente = ip?.IpNet?.Ip,
            MacCliente = mac?.CargueDetail?.MacWlan
        };

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<IntItemModel>>(TypesUrl);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Types = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());

            var setup = await _repository.GetAsync<ContractBindSetupDTO>($"{SetupUrl}/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(setup))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var datos = setup.Response;

            if (datos is null || !datos.CanCreate)
            {
                await _alertService.WarningAsync(
                    "IpBinding de acceso",
                    datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            GuardarConexion(datos.ServerId, datos.ServerName, datos.ServerIp, datos.Usuario,
                datos.Clave, datos.ApiPort, datos.NombreCliente);

            //Lo que manda es lo que dice el servidor, no lo que traia la pantalla
            Entity.IpCliente = datos.IpCliente;
            Entity.MacCliente = datos.MacCliente;
            Entity.IpServer = datos.ServerIp;
            Entity.ServerName = datos.ServerName;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Editar es la excepcion: el IpBinding es la unica pieza que se cambia sin quitarla.
    // Aqui no hay nada que calcular, solo hace falta saber con quien hablar.
    public async Task InitializeEditAsync(Guid contractClientId, Guid id)
    {
        SaveText = "Guardar el IpBinding";

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<IntItemModel>>(TypesUrl);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Types = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());

            var actual = await _repository.GetAsync<ContractBind>($"api/v1/contractbinds/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(actual))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = actual.Response ?? new ContractBind { ContractBindId = id, ContractClientId = contractClientId };

            var conexion = await _repository.GetAsync<ContractMkConnectionDTO>($"{ConnUrl}/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(conexion))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var datos = conexion.Response;

            if (datos is null || !datos.CanConnect)
            {
                await _alertService.WarningAsync(
                    "IpBinding de acceso",
                    datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            GuardarConexion(datos.ServerId, datos.ServerName, datos.ServerIp, datos.Usuario,
                datos.Clave, datos.ApiPort, datos.NombreCliente);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Entity.HotSpotTypeId <= 0)
        {
            await _alertService.WarningAsync("IpBinding de acceso", "Debe elegir el tipo de acceso.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(_blocked) || string.IsNullOrWhiteSpace(_serverIp))
        {
            await _alertService.WarningAsync("IpBinding de acceso", _blocked ?? "Falta el servidor del contrato.");
            return;
        }

        //El equipo pide el NOMBRE del tipo de acceso, no su id
        var tipo = Types.FirstOrDefault(x => x.Value == Entity.HotSpotTypeId)?.Name;

        if (string.IsNullOrWhiteSpace(tipo))
        {
            await _alertService.WarningAsync("IpBinding de acceso", "Debe elegir el tipo de acceso.");
            return;
        }

        IsSaving = true;

        try
        {
            //===== La orden al equipo, por la red LAN =====

            var server = new Server
            {
                ServerId = _serverId,
                ServerName = _serverName,
                Usuario = _usuario,
                Clave = _clave,
                ApiPort = _apiPort,
                IpNetwork = new IpNetwork { Ip = _serverIp }
            };

            bool esNuevo = Entity.ContractBindId == Guid.Empty;
            string bindIndex = Entity.MikrotikId ?? string.Empty;

            var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
            {
                int total;
                int rest;
                string idmk;

                if (esNuevo)
                {
                    mikrotik.Send("/ip/hotspot/ip-binding/add");
                    mikrotik.Send("=address=" + Entity.IpCliente);
                    mikrotik.Send("=to-address=" + Entity.IpCliente);
                    mikrotik.Send("=comment=" + _nombreCliente);
                    mikrotik.Send("=mac-address=" + Entity.MacCliente);
                    mikrotik.Send("=server=" + "all");
                    mikrotik.Send("=type=" + tipo);
                    mikrotik.Send("/ip/hotspot/ip-binding/print", true);

                    foreach (var item in mikrotik.Read())
                    {
                        idmk = item;
                        total = idmk.Length;
                        rest = total - 10;
                        bindIndex = idmk.Substring(10, rest);
                    }
                }
                else
                {
                    mikrotik.Send("/ip/hotspot/ip-binding/set");
                    mikrotik.Send("=.id=" + Entity.MikrotikId);
                    mikrotik.Send("=address=" + Entity.IpCliente);
                    mikrotik.Send("=to-address=" + Entity.IpCliente);
                    mikrotik.Send("=comment=" + _nombreCliente);
                    mikrotik.Send("=mac-address=" + Entity.MacCliente);
                    mikrotik.Send("=server=" + "all");
                    mikrotik.Send("=type=" + tipo);
                    mikrotik.Send("/ip/hotspot/ip-binding/print", true);

                    foreach (var item in mikrotik.Read())
                    {
                        idmk = item;
                        total = idmk.Length;
                        rest = total - 10;
                    }
                }
            });

            if (!resultado.WasExecuted)
            {
                await _alertService.ErrorAsync("IpBinding de acceso", resultado.Message);
                return;
            }

            //===== El equipo ya quedo escrito: ahora se guarda el registro =====

            Entity.MikrotikId = bindIndex;

            var response = await _repository.PostAsync(BaseUrl, Entity);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private void GuardarConexion(Guid serverId, string? serverName, string? serverIp,
        string? usuario, string? clave, int apiPort, string? nombreCliente)
    {
        _serverId = serverId;
        _serverName = serverName;
        _serverIp = serverIp;
        _usuario = usuario;
        _clave = clave;
        _apiPort = apiPort;
        _nombreCliente = nombreCliente;
    }
}

// La ubicacion del servicio: donde quedo instalado.
// Se pueden pegar las coordenadas tal como las copia el mapa ("4.60971, -74.08175") o
// escribirlas por separado.
public partial class ContractMapDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractmaps";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ContractMap _entity = new();

    [ObservableProperty]
    private string _coordinates = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    private bool _esEdicion;

    public ContractMapDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync(Guid contractClientId, Guid? id)
    {
        _esEdicion = id.HasValue && id.Value != Guid.Empty;

        if (!_esEdicion)
        {
            Entity = new ContractMap { ContractClientId = contractClientId };
            return;
        }

        var response = await _repository.GetAsync<ContractMap>($"{BaseUrl}/{contractClientId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Entity = response.Response ?? new ContractMap { ContractClientId = contractClientId };

        if (Entity.Latitude.HasValue && Entity.Longitude.HasValue)
        {
            Coordinates = $"{Entity.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, " +
                          $"{Entity.Longitude.Value.ToString(CultureInfo.InvariantCulture)}";
        }
    }

    // Pegar "lat, lng" de un tiron es como se hace en la practica: se copia del mapa
    partial void OnCoordinatesChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var partes = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (partes.Length != 2)
        {
            return;
        }

        if (decimal.TryParse(partes[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitud) &&
            decimal.TryParse(partes[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitud))
        {
            Entity.Latitude = latitud;
            Entity.Longitude = longitud;

            OnPropertyChanged(nameof(Entity));
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Entity.Latitude is null || Entity.Longitude is null)
        {
            await _alertService.WarningAsync(
                "Ubicacion",
                "Debe indicar la latitud y la longitud. Puede pegarlas juntas, como las copia el mapa.");
            return;
        }

        IsSaving = true;

        try
        {
            var response = _esEdicion
                ? await _repository.PutAsync(BaseUrl, Entity)
                : await _repository.PostAsync(BaseUrl, Entity);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Cambiar el estado del contrato.
//
// Es el UNICO punto donde se cambia, porque esta pantalla administra el MikroTik: un
// contrato no puede quedar suspendido y con servicio. Las transiciones permitidas las
// decide el BACKEND segun el estado actual: aqui no se arma ninguna regla.
public partial class ContractChangeStateDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractcontrols";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly LanguageService _languageService;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _states = new();

    [ObservableProperty]
    private int _newState;

    [ObservableProperty]
    private string _reason = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _contractClientId;
    private ContractState _currentState;

    public string CurrentStateText => _languageService.Text($"ContractState_{_currentState}");

    // Con una sola opcion no hay a donde moverse: el backend no permite ninguna transicion
    public bool HasTransitions => States.Count > 1;

    public bool HasNoTransitions => !HasTransitions;

    public ContractChangeStateDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        LanguageService languageService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _languageService = languageService;
    }

    public async Task InitializeAsync(Guid contractClientId, ContractState currentState)
    {
        _contractClientId = contractClientId;
        _currentState = currentState;

        OnPropertyChanged(nameof(CurrentStateText));

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<IntItemModel>>(
                $"{BaseUrl}/loadStateChangeOptions/{contractClientId}");

            if (await _responseHandler.HandleErrorAsync(response))
            {
                States = new ObservableCollection<IntItemModel>();
                return;
            }

            States = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());

            OnPropertyChanged(nameof(HasTransitions));
            OnPropertyChanged(nameof(HasNoTransitions));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (NewState <= 0)
        {
            await _alertService.WarningAsync("Cambiar el estado", "Debe elegir el estado nuevo.");
            return;
        }

        var nombre = States.FirstOrDefault(x => x.Value == NewState)?.Name ?? string.Empty;

        var confirmado = await _alertService.ConfirmAsync(
            "Cambiar el estado",
            $"El contrato pasara a {nombre}. Desea continuar?",
            "Cambiar");

        if (!confirmado)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var url = $"{BaseUrl}/changeState/{_contractClientId}/{NewState}" +
                      $"?motivo={Uri.EscapeDataString(Reason ?? string.Empty)}";

            var response = await _repository.PutAsync<object, Spix.Domain.EntitiesContratos.ContractClient>(url, new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
