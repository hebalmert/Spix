using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.ContractClient;
using Spix.AppWpf.Views.EntitiesContratos.ContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ContractClientEntity = Spix.Domain.EntitiesContratos.ContractClient;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;

// La configuracion del servicio de UN contrato.
//
// Es la pantalla mas grande del sistema y no es un formulario: es una LISTA DE REQUISITOS
// en cascada. Ocho piezas —servidor, IP, nodo, plan, MAC, ubicacion, y si la corporacion
// usa HotSpot tambien la Queue y el IpBinding— que hay que configurar antes de que el
// contrato pueda activarse.
//
// Dos reglas que vienen de la web y son las que hacen que esto no sea un CRUD:
//  1. Hay CANDADOS: mientras existan Queue o IpBinding no se puede tocar el servidor, la
//     IP ni el plan, porque esas piezas ya estan escritas en el MikroTik. Y la MAC no se
//     toca mientras exista el IpBinding.
//  2. Activar es lo unico que enciende el servicio, y el SERVIDOR revalida todo: si la
//     corporacion usa HotSpot y falta la Queue o el IpBinding, lo rechaza.
public partial class ContractControlDetailViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractcontrols";
    private const string ServerUrl = "api/v1/contractservers";
    private const string IpUrl = "api/v1/contractips";
    private const string NodeUrl = "api/v1/contractnodes";
    private const string PlanUrl = "api/v1/contractplans";
    private const string MacUrl = "api/v1/contractmacs";
    private const string MapUrl = "api/v1/contractmaps";
    private const string QueUrl = "api/v1/contractques";
    private const string BindUrl = "api/v1/contractbinds";
    private const string MikrotikUrl = "api/v1/connectionmikrotikcontrols";

    //El v2 es el API del escritorio: entrega los datos del equipo y guarda lo que ya se
    //escribio en el por la red LAN
    private const string QueSetupUrl = "api/v2/contractmksetup/que";
    private const string BindConnUrl = "api/v2/contractmksetup/conn";
    private const string BindSaveUrl = "api/v2/contractmksetup/bind";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly LanguageService _languageService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private ContractClientEntity? _contract;

    [ObservableProperty]
    private ContractServer? _server;

    [ObservableProperty]
    private ContractIp? _ip;

    [ObservableProperty]
    private ContractNode? _node;

    [ObservableProperty]
    private ContractPlan? _plan;

    [ObservableProperty]
    private ContractMac? _mac;

    [ObservableProperty]
    private ContractMap? _map;

    [ObservableProperty]
    private ContractQue? _queue;

    [ObservableProperty]
    private ContractBind? _bind;

    [ObservableProperty]
    private ObservableCollection<string> _missing = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _id;

    // Se pide al salir del detalle
    public event EventHandler? BackRequested;

    public ContractControlDetailViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        LanguageService languageService,
        ILocalMikrotikService mikrotikService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _languageService = languageService;
        _mikrotikService = mikrotikService;
    }

    //===================== Lo que hay y lo que falta =====================

    // La corporacion usa MikroTik en modo HotSpot: entonces el contrato necesita dos
    // piezas mas, la Queue de velocidad y el IpBinding de acceso.
    public bool UseHotSpot { get; private set; }

    public bool HasServer => Server is not null && Server.ContractServerId != Guid.Empty;

    public bool HasIp => Ip is not null && Ip.ContractIpId != Guid.Empty;

    public bool HasNode => Node is not null && Node.ContractNodeId != Guid.Empty;

    public bool HasPlan => Plan is not null && Plan.ContractPlanId != Guid.Empty;

    public bool HasMac => Mac is not null && Mac.ContractMacId != Guid.Empty;

    public bool HasMap => Map is not null && Map.ContractMapId != Guid.Empty;

    public bool HasQueue => Queue is not null && Queue.ContractQueId != Guid.Empty;

    public bool HasBind => Bind is not null && Bind.ContractBindId != Guid.Empty;

    // Mientras exista alguna de las dos, lo que ya esta en el MikroTik no se toca
    public bool HasHotSpotDependencies => HasQueue || HasBind;

    public int TotalItems => UseHotSpot ? 8 : 6;

    public int DoneItems => TotalItems - Missing.Count;

    public int ProgressPercent => TotalItems == 0 ? 0 : DoneItems * 100 / TotalItems;

    public string ProgressText => $"{DoneItems}/{TotalItems}";

    public bool IsComplete => Missing.Count == 0;

    public bool HasMissing => Missing.Count > 0;

    // Activar solo tiene sentido mientras el contrato esta en configuracion
    public bool CanActivate => Contract?.ContractState == ContractState.InProgress;

    //===================== Los candados =====================

    // El servidor, la IP y el plan ya estan escritos en el MikroTik
    public bool CanChangeServer => !HasHotSpotDependencies;

    public bool CanChangeIp => !HasHotSpotDependencies;

    public bool CanChangePlan => !HasHotSpotDependencies;

    // La MAC solo la sostiene el IpBinding
    public bool CanChangeMac => !HasBind;

    public string LockTip => "Elimine primero la Queue de velocidad y el IpBinding de acceso";

    public string LockTipMac => "Elimine primero el IpBinding de acceso";

    //===================== Ficha del contrato =====================

    public string Number => Contract is null ? string.Empty : $"#{Contract.ControlContrato}";

    public string ClientName => $"{Contract?.Client?.FirstName} {Contract?.Client?.LastName}".Trim();

    public string ClientDocument =>
        $"{Contract?.Client?.DocumentType?.DocumentName} {Contract?.Client?.Document}".Trim();

    public string? Phone => Contract?.PhoneNumber;

    public string? Email => Contract?.Client?.Email;

    public string Address =>
        $"{Contract?.Address} · {Contract?.Zone?.City?.Name} · Zona {Contract?.Zone?.ZoneName}";

    public string ContractorName => $"{Contract?.Contractor?.FirstName} {Contract?.Contractor?.LastName}".Trim();

    public string Created => Contract?.DateCreado.ToString("dd/MM/yyyy") ?? string.Empty;

    public string StatusText => Contract is null
        ? string.Empty
        : _languageService.Text($"ContractState_{Contract.ContractState}");

    public Brush StatusColor => Contract is null
        ? Brushes.Gray
        : ContractClient.ContractColors.Pincel(Contract.ContractState);

    public bool HasEquipment => Contract?.EquipoEmpres == true;

    public bool HasInvoice => Contract?.EnvoiceClient == true;

    public bool HasContract => Contract is not null;

    //===================== Lo que se ve en cada tarjeta =====================

    public string? ServerName => Server?.Server?.ServerName;

    public string? IpText => Ip?.IpNet?.Ip;

    public string? NodeName => Node?.Node?.NodesName;

    public string? PlanName => Plan?.Plan?.PlanName;

    public string? MacText => Mac?.CargueDetail?.MacWlan;

    public string? MapText => HasMap ? $"{Map!.Latitude}, {Map.Longitude}" : null;

    public string? QueueText => Queue?.TotalVelocidad;

    public string? BindText => Bind?.MacCliente ?? Bind?.CargueDetail?.MacWlan;

    public async Task InitializeAsync(Guid id)
    {
        _id = id;

        IsLoading = true;

        try
        {
            await CargarContratoAsync();
            await CargarModoMikrotikAsync();
            await CargarPiezasAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    //===================== Configurar cada pieza =====================

    // Las cuatro primeras se configuran igual: se elige uno de una lista y se guarda.
    // Cada una tiene su modal, todas comparten el flujo (ContractPieceDialogViewModel).
    [RelayCommand]
    private async Task SetServerAsync()
    {
        await AbrirPiezaAsync<ContractServerDialogView>("Servidor Gateway");
    }

    [RelayCommand]
    private async Task SetIpAsync()
    {
        await AbrirPiezaAsync<ContractIpDialogView>("IP del cliente");
    }

    [RelayCommand]
    private async Task SetNodeAsync()
    {
        await AbrirPiezaAsync<ContractNodeDialogView>("Nodo de acceso");
    }

    [RelayCommand]
    private async Task SetMacAsync()
    {
        await AbrirPiezaAsync<ContractMacDialogView>("MAC del equipo");
    }

    private async Task AbrirPiezaAsync<TView>(string titulo)
        where TView : System.Windows.Controls.UserControl, ISharedModalContent
    {
        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id
        };

        var result = await _modalService.ShowAsync<TView>(titulo, parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // El plan va aparte: son dos listas en cascada, categoria y luego plan
    [RelayCommand]
    private async Task SetPlanAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id
        };

        var result = await _modalService.ShowAsync<ContractPlanDialogView>("Plan del cliente", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    [RelayCommand]
    private async Task SetMapAsync()
    {
        await AbrirMapaAsync(false);
    }

    // La Ubicacion es de las dos que SI se editan sin quitarlas
    [RelayCommand]
    private async Task EditMapAsync()
    {
        await AbrirMapaAsync(true);
    }

    private async Task AbrirMapaAsync(bool esEdicion)
    {
        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id
        };

        if (esEdicion && HasMap)
        {
            parametros["Id"] = Map!.ContractMapId;
        }

        var titulo = esEdicion ? "Editar la ubicacion" : "Ubicacion del servicio";

        var result = await _modalService.ShowAsync<ContractMapDialogView>(titulo, parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // Ver donde quedo el cliente, y su nodo si lo tiene ubicado: asi se aprecia a que
    // distancia esta de su AP.
    [RelayCommand]
    private async Task ViewMapAsync()
    {
        if (!HasMap || Map?.Latitude is null || Map.Longitude is null)
        {
            await _alertService.WarningAsync("Ubicacion", "El contrato todavia no tiene coordenadas.");
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Latitude"] = Map.Latitude.Value,
            ["Longitude"] = Map.Longitude.Value
        };

        //El nodo se agrega solo si esta ubicado
        if (Node?.Node?.Latitude is not null && Node.Node.Longitude is not null)
        {
            parametros["NodeLatitude"] = Node.Node.Latitude.Value;
            parametros["NodeLongitude"] = Node.Node.Longitude.Value;
            parametros["NodeName"] = Node.Node.NodesName ?? "Nodo";
        }

        await _modalService.ShowAsync<ContractMapViewDialogView>("Ubicacion del servicio", parametros);
    }

    // La Queue y el IpBinding no se eligen: se arman con lo que ya quedo configurado y se
    // escriben en el MikroTik. Por eso sus modales solo muestran y confirman.
    [RelayCommand]
    private async Task SetQueueAsync()
    {
        //La Queue se arma con lo que YA quedo configurado: por eso se le pasan las piezas
        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id
        };

        if (Server is not null) parametros["Server"] = Server;
        if (Ip is not null) parametros["Ip"] = Ip;
        if (Plan is not null) parametros["Plan"] = Plan;

        var result = await _modalService.ShowAsync<ContractQueueDialogView>("Queue de velocidad", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    [RelayCommand]
    private async Task SetBindAsync()
    {
        //El IpBinding tambien: servidor, IP y la MAC del equipo instalado
        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id
        };

        if (Server is not null) parametros["Server"] = Server;
        if (Ip is not null) parametros["Ip"] = Ip;
        if (Mac is not null) parametros["Mac"] = Mac;

        var result = await _modalService.ShowAsync<ContractBindDialogView>("IpBinding de acceso", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // El IpBinding es la otra que se edita sin quitarla: se le cambia el tipo de acceso
    [RelayCommand]
    private async Task EditBindAsync()
    {
        if (!HasBind)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id,
            ["Id"] = Bind!.ContractBindId
        };

        if (Server is not null) parametros["Server"] = Server;
        if (Ip is not null) parametros["Ip"] = Ip;
        if (Mac is not null) parametros["Mac"] = Mac;

        var result = await _modalService.ShowAsync<ContractBindDialogView>("Editar el IpBinding", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    //===================== Quitar cada pieza =====================

    [RelayCommand]
    private async Task RemoveServerAsync()
    {
        await QuitarAsync(ServerUrl, Server?.ContractServerId, CanChangeServer, LockTip);
    }

    [RelayCommand]
    private async Task RemoveIpAsync()
    {
        await QuitarAsync(IpUrl, Ip?.ContractIpId, CanChangeIp, LockTip);
    }

    [RelayCommand]
    private async Task RemoveNodeAsync()
    {
        await QuitarAsync(NodeUrl, Node?.ContractNodeId, true, null);
    }

    [RelayCommand]
    private async Task RemovePlanAsync()
    {
        await QuitarAsync(PlanUrl, Plan?.ContractPlanId, CanChangePlan, LockTip);
    }

    [RelayCommand]
    private async Task RemoveMacAsync()
    {
        await QuitarAsync(MacUrl, Mac?.ContractMacId, CanChangeMac, LockTipMac);
    }

    [RelayCommand]
    private async Task RemoveMapAsync()
    {
        await QuitarAsync(MapUrl, Map?.ContractMapId, true, null);
    }

    // La Queue y el IpBinding no se quitan como las demas piezas: primero hay que borrarlos
    // del MikroTik, y eso lo hace el ESCRITORIO por la red LAN. Por eso tienen su propio
    // metodo con su propia porcion de MikroTik, en vez de pasar por QuitarAsync.
    [RelayCommand]
    private async Task RemoveQueueAsync()
    {
        if (Queue is null || Queue.ContractQueId == Guid.Empty)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Quitar la Queue de velocidad",
            "Se borrara del MikroTik. Esta accion no se puede deshacer.",
            "Quitar");

        if (!confirmado)
        {
            return;
        }

        //Quitar no es solo borrar la queue del cliente: el padre se queda con uno menos,
        //asi que hay que recalcularlo, y si se queda sin nadie tambien se borra
        var setup = await _repository.GetAsync<ContractQueRemoveSetupDTO>(
            $"{QueSetupUrl}/remove/{Queue.ContractQueId}");

        if (await _responseHandler.HandleErrorAsync(setup))
        {
            return;
        }

        var datos = setup.Response;

        if (datos is null || !datos.CanRemove)
        {
            await _alertService.WarningAsync(
                "Quitar la Queue de velocidad",
                datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

            return;
        }

        IsSaving = true;

        try
        {
            //===== El calculo, el mismo que hace el Backend =====

            //Los que QUEDAN colgados del padre. Si no queda ninguno, el padre sobra.
            int cantQueueClients = datos.ClientIps.Count;
            string ListaClientsIP = cantQueueClients == 0
                ? "0.0.0.0/0"
                : string.Join(", ", datos.ClientIps);

            int tasaReuso = datos.TasaReuso <= 0 ? 1 : datos.TasaReuso;
            int UpSpeed = datos.SpeedUpKbps;
            int DownSpeed = datos.SpeedDownKbps;
            int UpSpeedLimitAt = UpSpeed / tasaReuso;
            int DownSpeedLimitAt = DownSpeed / tasaReuso;
            int UpSpeedFather = cantQueueClients < tasaReuso + 1 ? UpSpeed : UpSpeedLimitAt * cantQueueClients;
            int DownSpeedFather = cantQueueClients < tasaReuso + 1 ? DownSpeed : DownSpeedLimitAt * cantQueueClients;

            bool hayPadre = !string.IsNullOrWhiteSpace(datos.ParentMkId);
            bool padreSeQuita = hayPadre && ListaClientsIP == "0.0.0.0/0";

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

            var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
            {
                int total;
                int rest;
                string idmk;

                mikrotik.Send("/queue/simple/remove");
                mikrotik.Send("=.id=" + datos.MikrotikId, true);

                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                }

                //Queda gente colgada del padre: se le recalcula y se le cambia el target
                if (hayPadre && !padreSeQuita)
                {
                    mikrotik.Send("/queue/simple/set");
                    mikrotik.Send("=.id=" + datos.ParentMkId);
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

                //El padre se quedo sin nadie: sobra
                if (padreSeQuita)
                {
                    mikrotik.Send("/queue/simple/remove");
                    mikrotik.Send("=.id=" + datos.ParentMkId, true);

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
                await _alertService.ErrorAsync("Quitar la Queue de velocidad", resultado.Message);
                return;
            }

            //===== El equipo ya quedo limpio: ahora se borra el registro =====

            var quitar = new ContractQueRemoveDTO
            {
                ContractQueId = datos.ContractQueId,
                ParentRemoved = padreSeQuita,
                ParentUp = $"{UpSpeed}k",
                ParentDown = $"{DownSpeed}k"
            };

            var response = await _repository.PostAsync($"{QueSetupUrl}/remove", quitar);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await RecargarAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task RemoveBindAsync()
    {
        if (Bind is null || Bind.ContractBindId == Guid.Empty)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Quitar el IpBinding de acceso",
            "Se borrara del MikroTik. Esta accion no se puede deshacer.",
            "Quitar");

        if (!confirmado)
        {
            return;
        }

        //Aqui no hay nada que calcular: solo hace falta saber con quien hablar
        var conexion = await _repository.GetAsync<ContractMkConnectionDTO>($"{BindConnUrl}/{_id}");
        if (await _responseHandler.HandleErrorAsync(conexion))
        {
            return;
        }

        var datos = conexion.Response;

        if (datos is null || !datos.CanConnect)
        {
            await _alertService.WarningAsync(
                "Quitar el IpBinding de acceso",
                datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

            return;
        }

        IsSaving = true;

        try
        {
            var server = new Server
            {
                ServerId = datos.ServerId,
                ServerName = datos.ServerName,
                Usuario = datos.Usuario,
                Clave = datos.Clave,
                ApiPort = datos.ApiPort,
                IpNetwork = new IpNetwork { Ip = datos.ServerIp }
            };

            var mikrotikId = Bind.MikrotikId;

            var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
            {
                int total;
                int rest;
                string idmk;

                mikrotik.Send("/ip/hotspot/ip-binding/remove");
                mikrotik.Send("=.id=" + mikrotikId, true);

                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                }
            });

            if (!resultado.WasExecuted)
            {
                await _alertService.ErrorAsync("Quitar el IpBinding de acceso", resultado.Message);
                return;
            }

            //===== El equipo ya quedo limpio: ahora se borra el registro =====

            var response = await _repository.DeleteAsync($"{BindSaveUrl}/{Bind.ContractBindId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await RecargarAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task QuitarAsync(string url, Guid? id, bool permitido, string? aviso)
    {
        if (id is null || id == Guid.Empty)
        {
            return;
        }

        //El candado: lo que ya esta escrito en el MikroTik no se quita por aqui
        if (!permitido)
        {
            await _alertService.WarningAsync("No se puede quitar", aviso ?? LockTip);
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Quitar",
            "Esta accion no se puede deshacer.",
            "Quitar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{url}/{id}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();
    }

    //===================== Activar y cambiar de estado =====================

    [RelayCommand]
    private async Task ActivateAsync()
    {
        if (!CanActivate)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Activar contrato",
            "Desea activar este contrato?",
            "Activar");

        if (!confirmado)
        {
            return;
        }

        IsSaving = true;

        try
        {
            //El servidor revalida todo: si falta algo, lo rechaza con su mensaje
            var response = await _repository.PostAsync<object, ContractClientEntity>(
                $"{BaseUrl}/{_id}/activate", new { });

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Activado", "El contrato quedo activo.");

            await RecargarAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    // El UNICO punto donde se cambia el estado del contrato, porque esta pantalla es la
    // que administra el MikroTik: no puede quedar suspendido y con servicio.
    [RelayCommand]
    private async Task ChangeStateAsync()
    {
        if (Contract is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["ContractClientId"] = _id,
            ["CurrentState"] = Contract.ContractState
        };

        var result = await _modalService.ShowAsync<ContractChangeStateDialogView>(
            "Cambiar el estado del contrato", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["Id"] = _id
        };

        var result = await _modalService.ShowAsync<EditContractClientDialogView>("Editar contrato", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    //===================== Carga =====================

    private async Task RecargarAsync()
    {
        IsLoading = true;

        try
        {
            await CargarContratoAsync();
            await CargarPiezasAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarContratoAsync()
    {
        var response = await _repository.GetAsync<ContractClientEntity>($"{BaseUrl}/{_id}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Contract = response.Response;
    }

    private async Task CargarModoMikrotikAsync()
    {
        var response = await _repository.GetAsync<List<ConnectionMikrotikControl>>(
            $"{MikrotikUrl}?page=1&recordsnumber=1");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            UseHotSpot = false;
            return;
        }

        UseHotSpot = response.Response?.FirstOrDefault()?.MikrotikControlType == MikrotikControlType.HotSpot;
    }

    // Cada pieza se pide SOLO si el contrato dice que la tiene: el contrato trae los
    // contadores, asi no se hacen ocho llamadas para nada.
    private async Task CargarPiezasAsync()
    {
        Server = Contract?.ControlServerCount > 0 ? await PedirAsync<ContractServer>(ServerUrl) : null;
        Ip = Contract?.ControlIpCount > 0 ? await PedirAsync<ContractIp>(IpUrl) : null;
        Node = Contract?.ControlNodeCount > 0 ? await PedirAsync<ContractNode>(NodeUrl) : null;
        Plan = Contract?.ControlPlanCount > 0 ? await PedirAsync<ContractPlan>(PlanUrl) : null;
        Mac = Contract?.ControlMacCount > 0 ? await PedirAsync<ContractMac>(MacUrl) : null;
        Map = Contract?.ControlMapCount > 0 ? await PedirAsync<ContractMap>(MapUrl) : null;

        if (UseHotSpot)
        {
            Queue = await PedirAsync<ContractQue>(QueUrl);
            Bind = await PedirAsync<ContractBind>(BindUrl);
        }
        else
        {
            Queue = null;
            Bind = null;
        }

        ArmarPendientes();
    }

    private async Task<T?> PedirAsync<T>(string url) where T : class
    {
        var response = await _repository.GetAsync<T>($"{url}/{_id}");

        //Que no exista no es un error que valga contar: la pieza simplemente falta
        return response.Error ? null : response.Response;
    }

    private void ArmarPendientes()
    {
        var faltan = new List<string>();

        if (!HasServer) faltan.Add("Servidor Gateway");
        if (!HasIp) faltan.Add("IP del cliente");
        if (!HasNode) faltan.Add("Nodo de acceso");
        if (!HasPlan) faltan.Add("Plan del cliente");
        if (!HasMac) faltan.Add("MAC del equipo");
        if (!HasMap) faltan.Add("Ubicacion");
        if (UseHotSpot && !HasQueue) faltan.Add("Queue de velocidad");
        if (UseHotSpot && !HasBind) faltan.Add("IpBinding de acceso");

        Missing = new ObservableCollection<string>(faltan);

        Refrescar();
    }

    // Un solo sitio para avisar de todo lo que depende de las piezas
    private void Refrescar()
    {
        foreach (var propiedad in new[]
        {
            nameof(HasContract), nameof(Number), nameof(ClientName), nameof(ClientDocument),
            nameof(Phone), nameof(Email), nameof(Address), nameof(ContractorName), nameof(Created),
            nameof(StatusText), nameof(StatusColor), nameof(HasEquipment), nameof(HasInvoice),
            nameof(UseHotSpot), nameof(HasServer), nameof(HasIp), nameof(HasNode), nameof(HasPlan),
            nameof(HasMac), nameof(HasMap), nameof(HasQueue), nameof(HasBind),
            nameof(HasHotSpotDependencies), nameof(CanChangeServer), nameof(CanChangeIp),
            nameof(CanChangePlan), nameof(CanChangeMac), nameof(TotalItems), nameof(DoneItems),
            nameof(ProgressPercent), nameof(ProgressText), nameof(IsComplete), nameof(HasMissing),
            nameof(CanActivate), nameof(ServerName), nameof(IpText), nameof(NodeName),
            nameof(PlanName), nameof(MacText), nameof(MapText), nameof(QueueText), nameof(BindText)
        })
        {
            OnPropertyChanged(propiedad);
        }
    }
}
