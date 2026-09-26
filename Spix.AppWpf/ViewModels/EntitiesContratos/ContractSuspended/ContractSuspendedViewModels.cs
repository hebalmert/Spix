using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Network;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.ContractSuspended;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspended;

// Contratos suspendidos: el registro de las suspensiones, con su monto al momento de
// suspender. La fila NO se borra al reactivar, se cierra y queda como historia.
//
// OJO con reactivar, que es donde esta el trabajo de verdad: hay que devolverle el acceso
// al cliente en el MikroTik, y eso lo hace el ESCRITORIO por la red LAN, porque el cliente
// puede no tener IP publica. Por eso la porcion de MikroTik esta replicada aqui adentro.
public partial class ContractSuspendedIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractsuspended";
    private const string MkUrl = "api/v2/contractsuspendedmk";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private ObservableCollection<SuspendedRow> _rows = new();

    [ObservableProperty]
    private string? _filter;

    [ObservableProperty]
    private DateTime? _desde;

    [ObservableProperty]
    private DateTime? _hasta;

    //Por defecto se ven los que siguen suspendidos, que es lo que se atiende a diario
    [ObservableProperty]
    private bool _soloAbiertas = true;

    [ObservableProperty]
    private int _openCount;

    [ObservableProperty]
    private decimal _openAmount;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _message;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    //El encabezado: cuantos hay suspendidos hoy y cuanto suman
    public string OpenText => $"{OpenCount} suspendidos · {OpenAmount.ToString("N2", CultureInfo.CurrentCulture)}";

    public string TotalText => $"Total {TotalAmount.ToString("N2", CultureInfo.CurrentCulture)}";

    public string CountText => $"{TotalCount} registros en el filtro";

    public ContractSuspendedIndexViewModel(
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

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    partial void OnMessageChanged(string? value) => OnPropertyChanged(nameof(HasMessage));

    partial void OnOpenCountChanged(int value) => OnPropertyChanged(nameof(OpenText));

    partial void OnOpenAmountChanged(decimal value) => OnPropertyChanged(nameof(OpenText));

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(CountText));

    partial void OnTotalAmountChanged(decimal value) => OnPropertyChanged(nameof(TotalText));

    //Los filtros recargan solos: es una lista corta y sin paginacion
    partial void OnDesdeChanged(DateTime? value) => _ = LoadAsync();

    partial void OnHastaChanged(DateTime? value) => _ = LoadAsync();

    partial void OnSoloAbiertasChanged(bool value) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;
        Message = null;

        try
        {
            var url = $"{BaseUrl}/records?soloAbiertas={SoloAbiertas}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            if (Desde.HasValue)
            {
                url += $"&desde={Desde.Value:yyyy-MM-dd}";
            }

            if (Hasta.HasValue)
            {
                url += $"&hasta={Hasta.Value:yyyy-MM-dd}";
            }

            var response = await _repository.GetAsync<SuspendedListDTO>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var datos = response.Response ?? new SuspendedListDTO();

            Rows = new ObservableCollection<SuspendedRow>(datos.Records.Select(x => new SuspendedRow(x)));
            OpenCount = datos.OpenCount;
            OpenAmount = datos.OpenAmount;
            TotalCount = datos.TotalCount;
            TotalAmount = datos.TotalAmount;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    //El texto llega por el enlace de SearchText, NO por parametro: el buscador de los
    //index ejecuta el comando sin nada, igual que en el resto del escritorio
    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync();
    }

    //Suspender: se busca un contrato activo y el escritorio hace el resto
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<ContractSuspendedDialogView>("Suspender contrato");

        if (result.Succeeded)
        {
            await LoadAsync();
            await _alertService.SuccessAsync("Suspension", "El contrato quedo suspendido.");
        }
    }

    //Quien registro la suspension no ocupa una columna: se consulta aqui
    [RelayCommand]
    private async Task AuditAsync(SuspendedRow? item)
    {
        if (item is null)
        {
            return;
        }

        var lineas = new List<string>
        {
            $"Registrada por: {item.UserByName}",
            $"Fecha: {item.DateSuspended.ToLocalTime():dd/MM/yyyy HH:mm}",
            $"Origen: {(item.Origin == SuspendedOrigin.Corte ? "Corte" : "Manual")}"
        };

        if (!string.IsNullOrWhiteSpace(item.Motivo))
        {
            lineas.Add($"Motivo: {item.Motivo}");
        }

        if (item.DateReactivated.HasValue)
        {
            lineas.Add($"Reactivada por: {item.UserByNameReactivated}");
            lineas.Add($"Fecha de reactivacion: {item.DateReactivated.Value.ToLocalTime():dd/MM/yyyy HH:mm}");
        }

        await _alertService.WarningAsync("Rastro de la suspension", string.Join(Environment.NewLine, lineas));
    }

    [RelayCommand]
    private async Task ReasonAsync(SuspendedRow? item)
    {
        if (item is null || string.IsNullOrWhiteSpace(item.Motivo))
        {
            return;
        }

        await _alertService.WarningAsync("Motivo", item.Motivo);
    }

    // Reactivar: el acceso vuelve a bypassed en el equipo y el contrato a Activo.
    //
    // Aqui esta la porcion de MikroTik replicada: el escritorio abre la conexion por la
    // red LAN y manda el set. El v2 solo le dice con quien hablar, y despues guarda.
    [RelayCommand]
    private async Task ActivateAsync(SuspendedRow? item)
    {
        if (item is null || item.DateReactivated.HasValue)
        {
            return;
        }

        //Se pregunta antes de tocar el equipo: un clic por error no puede devolverle el
        //servicio a un cliente suspendido
        var confirmado = await _alertService.ConfirmAsync(
            "Reactivar el servicio",
            $"Desea reactivar el contrato {item.ControlContrato} de {item.ClientName}?",
            "Reactivar");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var setup = await _repository.GetAsync<ReactivateMkSetupDTO>($"{MkUrl}/{item.ContractClientId}/activate");
            if (await _responseHandler.HandleErrorAsync(setup))
            {
                return;
            }

            var datos = setup.Response;

            if (datos is null || !datos.CanReactivate)
            {
                await _alertService.WarningAsync(
                    "Reactivar el servicio",
                    datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

                return;
            }

            //===== La orden al equipo, por la red LAN =====
            //Sin HotSpot no hay nada que escribir: solo cambia el estado

            if (datos.UsaHotSpot)
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

                var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
                {
                    //En un set solo hace falta el id y lo que cambia
                    mikrotik.Send("/ip/hotspot/ip-binding/set");
                    mikrotik.Send("=.id=" + datos.MkIndex);
                    mikrotik.Send("=type=" + datos.TipoBypassed, true);
                    mikrotik.Read();
                });

                if (!resultado.WasExecuted)
                {
                    await _alertService.ErrorAsync(
                        "No se pudo conectar con MikroTik",
                        $"{resultado.Message} El contrato continuara suspendido.");

                    return;
                }
            }

            //===== El equipo ya quedo escrito: ahora se guarda el rastro =====

            var response = await _repository.PostAsync($"{MkUrl}/{item.ContractClientId}/activate", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await LoadAsync();
            await _alertService.SuccessAsync("Reactivar el servicio", "El contrato quedo activo.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// Suspender un contrato: se escribe el nombre o la cedula, se elige el contrato activo y
// el ESCRITORIO bloquea el acceso en el equipo por la red LAN.
//
// Como en el resto del escritorio, la porcion de MikroTik esta replicada aqui adentro y el
// v2 solo entrega los datos y guarda el rastro.
public partial class ContractSuspendedDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractsuspended";
    private const string MkUrl = "api/v2/contractsuspendedmk";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly ILocalMikrotikService _mikrotikService;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<ActiveContractDTO> _candidates = new();

    [ObservableProperty]
    private ActiveContractDTO? _selected;

    [ObservableProperty]
    private string? _motivo;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _noResults;

    //Minimo de letras y pausa antes de consultar, igual que el autocompletar de la web
    private const int MinimoParaBuscar = 3;
    private const int PausaMs = 500;

    //Cada tecla cancela la consulta anterior: solo viaja la ultima
    private CancellationTokenSource? _cts;

    public bool HasSelected => Selected is not null;

    public string SelectedText => Selected is null
        ? string.Empty
        : $"#{Selected.ControlContrato} · {Selected.ClientName}   ({Selected.PlanName} · {Selected.PlanAmount:N2})";

    public ContractSuspendedDialogViewModel(
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

    partial void OnSelectedChanged(ActiveContractDTO? value)
    {
        OnPropertyChanged(nameof(HasSelected));
        OnPropertyChanged(nameof(SelectedText));
    }

    // Solo se ofrecen contratos ACTIVOS: los demas no se pueden suspender.
    //
    // Se espera medio segundo antes de consultar, igual que la web: sin eso se dispara una
    // consulta por tecla. Y tiene que admitir varias a la vez, porque si no el comando
    // DESCARTA las teclas que llegan mientras hay una consulta en vuelo, y escribiendo
    // rapido solo se busca por las primeras letras.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchAsync(string? texto)
    {
        SearchText = texto;
        Selected = null;

        _cts?.Cancel();

        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < MinimoParaBuscar)
        {
            Candidates = new ObservableCollection<ActiveContractDTO>();
            IsSearching = false;
            NoResults = false;
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(PausaMs, token);
        }
        catch (TaskCanceledException)
        {
            //Siguio escribiendo: esta consulta ya no vale
            return;
        }

        IsSearching = true;
        NoResults = false;

        var response = await _repository.GetAsync<List<ActiveContractDTO>>(
            $"{BaseUrl}/active?filter={Uri.EscapeDataString(texto.Trim())}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        IsSearching = false;

        if (await _responseHandler.HandleErrorAsync(response))
        {
            Candidates = new ObservableCollection<ActiveContractDTO>();
            return;
        }

        Candidates = new ObservableCollection<ActiveContractDTO>(response.Response ?? new List<ActiveContractDTO>());

        //Que no haya ninguno tiene que DECIRSE: sin aviso se ve igual que un buscador roto
        NoResults = Candidates.Count == 0;
    }

    [RelayCommand]
    private void Select(ActiveContractDTO? item)
    {
        if (item is null)
        {
            return;
        }

        //La lista no se toca aqui: el componente ya cerro el popup y solto la marca antes
        //de avisar, y cambiarle los items dentro de su propio evento es buscar problemas
        Selected = item;

        _cts?.Cancel();
        IsSearching = false;
        NoResults = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Selected is null)
        {
            await _alertService.WarningAsync("Suspender contrato", "Debe elegir el contrato.");
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Suspender contrato",
            $"Desea suspender el contrato {Selected.ControlContrato} de {Selected.ClientName}?",
            "Suspender");

        if (!confirmado)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var setup = await _repository.GetAsync<SuspendMkSetupDTO>($"{MkUrl}/{Selected.ContractClientId}/suspend");
            if (await _responseHandler.HandleErrorAsync(setup))
            {
                return;
            }

            var datos = setup.Response;

            if (datos is null || !datos.CanSuspend)
            {
                await _alertService.WarningAsync(
                    "Suspender contrato",
                    datos?.Blocked ?? "No fue posible obtener los datos del servidor.");

                return;
            }

            //===== Las ordenes al equipo, por la red LAN =====
            //Sin HotSpot no hay nada que escribir: solo cambia el estado

            if (datos.UsaHotSpot)
            {
                //Una conexion por servidor, no una por binding
                foreach (var grupo in datos.Bindings.GroupBy(x => x.ServerId))
                {
                    var primero = grupo.First();

                    var server = new Server
                    {
                        ServerId = primero.ServerId,
                        ServerName = primero.ServerName,
                        Usuario = primero.Usuario,
                        Clave = primero.Clave,
                        ApiPort = primero.ApiPort,
                        IpNetwork = new IpNetwork { Ip = primero.ServerIp }
                    };

                    var bindings = grupo.ToList();

                    var resultado = await _mikrotikService.ExecuteAsync(server, mikrotik =>
                    {
                        foreach (var binding in bindings)
                        {
                            //El acceso queda en regular: el cliente cae en el portal del HotSpot
                            mikrotik.Send("/ip/hotspot/ip-binding/set");
                            mikrotik.Send("=.id=" + binding.MikrotikId);
                            mikrotik.Send("=address=" + binding.IpCliente);
                            mikrotik.Send("=to-address=" + binding.IpCliente);
                            mikrotik.Send("=comment=" + datos.NombreCliente);
                            mikrotik.Send("=mac-address=" + binding.MacCliente);
                            mikrotik.Send("=server=all");
                            mikrotik.Send("=type=" + datos.TipoRegular, true);
                            mikrotik.Read();
                        }
                    });

                    if (!resultado.WasExecuted)
                    {
                        await _alertService.ErrorAsync(
                            "No se pudo conectar con MikroTik",
                            $"{resultado.Message} El contrato NO quedo suspendido.");

                        return;
                    }
                }
            }

            //===== El equipo ya quedo escrito: ahora se guarda el rastro =====

            var url = $"{MkUrl}/{Selected.ContractClientId}/suspend" +
                      $"?motivo={Uri.EscapeDataString(Motivo ?? string.Empty)}";

            var response = await _repository.PostAsync(url, new { });
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

// Una fila de la tabla, ya lista para pintar. El escritorio no calcula nada en el XAML:
// el texto, el color y lo que se puede hacer se resuelven aqui, una sola vez.
public class SuspendedRow
{
    public SuspendedRecordDTO Item { get; }

    public Guid ContractClientId => Item.ContractClientId;

    public long ControlContrato => Item.ControlContrato;

    public string? ClientName => Item.ClientName;

    public string? ClientDocument => Item.ClientDocument;

    public string? ContractAddress => Item.ContractAddress;

    // La ciudad y la zona van juntas debajo de la direccion
    public string Place => string.Join(" · ",
        new[] { Item.CityName, Item.ZoneName }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string? PlanName => Item.PlanName;

    public decimal PlanAmount => Item.PlanAmount;

    public string DateSuspendedText => Item.DateSuspended.ToLocalTime().ToString("dd/MM/yyyy");

    public string OriginText => Item.Origin == SuspendedOrigin.Corte ? "Corte" : "Manual";

    // El corte es automatico y la suspension a mano no: se distinguen de un vistazo
    public Brush OriginColor => Application.Current.TryFindResource(
        Item.Origin == SuspendedOrigin.Corte ? "BrushContractCancelled" : "BrushContractInProgress") as Brush
        ?? Brushes.Gray;

    // Mientras no tenga fecha de reactivacion, el cliente sigue sin servicio
    public bool IsOpen => Item.DateReactivated is null;

    // La pastilla pinta el "off" en rojo: suspendido es justo el estado que alarma
    public bool IsReactivated => Item.DateReactivated is not null;

    public bool HasReason => !string.IsNullOrWhiteSpace(Item.Motivo);

    public string? Motivo => Item.Motivo;

    public string? UserByName => Item.UserByName;

    public string? UserByNameReactivated => Item.UserByNameReactivated;

    public DateTime DateSuspended => Item.DateSuspended;

    public DateTime? DateReactivated => Item.DateReactivated;

    public SuspendedOrigin Origin => Item.Origin;

    public SuspendedRow(SuspendedRecordDTO item)
    {
        Item = item;
    }
}
