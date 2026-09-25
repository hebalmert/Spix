using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.Services.Session;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;

// Una fila del listado, YA LISTA PARA PINTAR.
//
// La pantalla no calcula nada: ni la urgencia de la fecha, ni el color del estado, ni si
// la solicitud se puede borrar. Es la misma idea de los combos —la lista se arma antes de
// llegar a la vista— y evita llenar el XAML de convertidores.
public class ServiceRequestRow
{
    public ServiceRequestDto Item { get; }

    public long Number => Item.RequestNumber;

    public string CreatedText => Item.CreatedAtUtc.ToLocalTime().ToString("dd/MM/yyyy");

    public string ClientName => Item.ClientFullName;

    public string ClientMeta => $"Contrato {Item.ControlContrato} · {Item.ZoneName}";

    public string Reason => Item.ClientReason;

    public string? Address => Item.Address;

    public string? TechnicianName => Item.TechnicianName;

    public string? ScheduledText => Item.ScheduledAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public string WhenText { get; }

    public Brush WhenBack { get; }

    public Brush WhenForeground { get; }

    public string StatusText { get; }

    public Brush StatusColor { get; }

    // Una solicitud cerrada no se borra: es la misma regla de la web
    public bool CanDelete => Item.ScheduleStatus != ScheduleStatus.Completed;

    public ServiceRequestRow(ServiceRequestDto item, IReadOnlyDictionary<int, string> estados)
    {
        Item = item;

        WhenText = TextoDeCuando(item);

        var urgencia = Urgencia(item);
        WhenBack = Pincel($"BrushWhen{urgencia}Back");
        WhenForeground = Pincel($"BrushWhen{urgencia}Text");

        //El nombre del estado lo manda el backend traducido, en la misma lista de las
        //pildoras de filtro: aqui no se traduce nada a mano.
        StatusText = estados.TryGetValue((int)item.ScheduleStatus, out var nombre)
            ? nombre
            : item.ScheduleStatus.ToString();

        StatusColor = Pincel(ClaveDeColor(item.ScheduleStatus));
    }

    // La fecha programada dice que tan urgente es, no solo cuando es
    private static string TextoDeCuando(ServiceRequestDto item)
    {
        //Mientras el cliente la pide y nadie la revisa, no hay cuando
        if (item.ScheduleStatus == ScheduleStatus.Requested)
        {
            return "Sin asignar";
        }

        if (item.ScheduleStatus == ScheduleStatus.Completed ||
            item.ScheduleStatus == ScheduleStatus.PhoneResolved)
        {
            return "Cerrada";
        }

        if (item.ScheduledAtUtc is null)
        {
            return "Sin asignar";
        }

        var fecha = item.ScheduledAtUtc.Value.ToLocalTime();
        var dias = (fecha.Date - DateTime.Now.Date).Days;

        if (dias == 0)
        {
            return $"Hoy {fecha:HH:mm}";
        }

        if (dias == 1)
        {
            return "Manana";
        }

        if (dias < 0)
        {
            return $"Vencida {Math.Abs(dias)}d";
        }

        return $"En {dias}d";
    }

    private static string Urgencia(ServiceRequestDto item)
    {
        if (item.ScheduleStatus == ScheduleStatus.Completed ||
            item.ScheduleStatus == ScheduleStatus.PhoneResolved)
        {
            return "Done";
        }

        if (item.ScheduledAtUtc is null)
        {
            return "Next";
        }

        var dias = (item.ScheduledAtUtc.Value.ToLocalTime().Date - DateTime.Now.Date).Days;

        if (dias < 0)
        {
            return "Late";
        }

        return dias == 0 ? "Today" : "Next";
    }

    private static string ClaveDeColor(ScheduleStatus estado)
    {
        return estado switch
        {
            ScheduleStatus.Requested => "BrushScheduleRequested",
            ScheduleStatus.PhoneResolved => "BrushSchedulePhoneResolved",
            ScheduleStatus.Pending => "BrushSchedulePending",
            ScheduleStatus.InProgress => "BrushScheduleInProgress",
            ScheduleStatus.OnHold => "BrushScheduleOnHold",
            ScheduleStatus.Rescheduled => "BrushScheduleRescheduled",
            ScheduleStatus.Completed => "BrushScheduleCompleted",
            ScheduleStatus.Cancelled => "BrushScheduleCancelled",
            _ => "BrushScheduleUnknown"
        };
    }

    // Los colores siguen viviendo en Colors.xaml: aqui solo se elige cual
    private static Brush Pincel(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}

// Solicitudes de servicio: las visitas tecnicas del cliente.
// Replicado de la pantalla /servicerequests de la web.
//
// El tablero y las pildoras de filtro los arma el BACKEND: los cinco numeros salen sobre
// todo lo que el usuario puede ver, no sobre la pagina en pantalla, y la lista de estados
// llega con su "Todas" en la posicion 0 y ya traducida.
public partial class ServiceRequestIndexViewModel : PagedListViewModel<ServiceRequestDto>
{
    private const string BaseUrl = "api/v1/servicerequests";
    private const string StatusUrl = "/api/v1/schedulecontrol/loadStatusFilter";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly IUserSessionService _sessionService;
    private readonly NavigationService _navigationService;

    //El estado elegido viaja en la direccion: cero es "todas"
    protected override string Endpoint => $"{BaseUrl}?status={StatusFilter}";

    [ObservableProperty]
    private ObservableCollection<ServiceRequestRow> _rows = new();

    [ObservableProperty]
    private ObservableCollection<ServiceRequestStatusChip> _statuses = new();

    [ObservableProperty]
    private ServiceRequestSummaryDto? _summary;

    private int _statusFilter;

    public int StatusFilter
    {
        get => _statusFilter;
        private set
        {
            if (_statusFilter == value)
            {
                return;
            }

            _statusFilter = value;
            OnPropertyChanged();

            //Las pildoras se vuelven a armar: la marcada es la elegida
            PintarPildoras();
        }
    }

    public bool HasSummary => Summary is not null;

    private readonly Dictionary<int, string> _nombresDeEstado = new();

    private List<IntItemModel> _estados = new();

    // El tecnico atiende visitas, no las registra: es la misma regla de la web
    public bool CanCreate =>
        _sessionService.Role.Contains("Administrator", StringComparison.OrdinalIgnoreCase) ||
        _sessionService.Role.Contains("Auxiliar", StringComparison.OrdinalIgnoreCase);

    public ServiceRequestIndexViewModel(
        IPagedEntityService<ServiceRequestDto> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler,
        IUserSessionService sessionService,
        NavigationService navigationService)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
        _sessionService = sessionService;
        _navigationService = navigationService;
    }

    // La orden es una PANTALLA: las fotos, los servicios y los comentarios viven ahi.
    // Es demasiado para un modal, igual que en la web.
    [RelayCommand]
    private void OpenOrder(ServiceRequestRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        _navigationService.Show<ServiceRequestOrderView>(
            "Orden de trabajo",
            $"Operaciones / Solicitud #{fila.Number}",
            vista =>
            {
                vista.Prepare(fila.Item.ServiceRequestId);

                //Al cerrar o salir de la orden se vuelve al listado, recargado
                vista.BackRequested += (_, _) => VolverAlListado();
            });
    }

    private void VolverAlListado()
    {
        _navigationService.Show<Views.EntitiesSchedule.ServiceRequest.ServiceRequestIndexView>(
            "Solicitudes de servicio",
            "Operaciones / Solicitudes de servicio");
    }

    // Rastro de la solicitud: cuando se creo, quien la cerro y cuando
    [RelayCommand]
    private async Task AuditAsync(ServiceRequestRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var item = fila.Item;

        var lineas = new List<string>
        {
            $"Creada: {item.CreatedAtUtc.ToLocalTime():dd/MM/yyyy HH:mm}",
            $"Programada: {Texto(item.ScheduledAtUtc)}",
            $"Tecnico: {Texto(item.TechnicianName)}",
            $"Cerrada por: {Texto(item.UsuarioOwnerCompleted)}",
            $"Fecha de cierre: {Texto(item.CompletedAtUtc)}"
        };

        await _alertService.SuccessAsync($"Solicitud #{item.RequestNumber}", string.Join(Environment.NewLine, lineas));
    }

    private static string Texto(DateTime? fecha)
    {
        return fecha is null ? "-" : fecha.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    private static string Texto(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? "-" : valor;
    }

    // Registrar una solicitud: lo unico que sigue siendo modal, porque es corto
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateServiceRequestDialogView>("Nueva solicitud");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await LoadSummaryAsync();

        await _alertService.SuccessAsync("Guardada", "La solicitud fue registrada correctamente.");
    }

    partial void OnSummaryChanged(ServiceRequestSummaryDto? value)
    {
        OnPropertyChanged(nameof(HasSummary));
    }

    // Las pildoras y el tablero se piden al abrir; la lista va detras
    public async Task InitializeAsync()
    {
        await LoadStatusesAsync();
        await LoadSummaryAsync();
        await LoadAsync();
    }

    // Cada carga arma las filas ya resueltas para la pantalla
    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<ServiceRequestRow>(
            Items.Select(x => new ServiceRequestRow(x, _nombresDeEstado)));

        return Task.CompletedTask;
    }

    // Cero es "todas"; al cambiar de pildora se vuelve a la primera pagina
    [RelayCommand]
    private async Task FilterByStatusAsync(string? valor)
    {
        if (!int.TryParse(valor, out var estado))
        {
            return;
        }

        StatusFilter = estado;

        await LoadAsync(1);
    }

    [RelayCommand]
    private async Task DeleteAsync(ServiceRequestRow? fila)
    {
        if (fila is null || !fila.CanDelete)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar solicitud",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.Item.ServiceRequestId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await LoadSummaryAsync();

        await _alertService.SuccessAsync("Eliminada", "La solicitud fue eliminada correctamente.");
    }

    private async Task LoadStatusesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>(StatusUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        _estados = response.Response ?? new List<IntItemModel>();

        //El mismo listado sirve para poner el nombre traducido en la insignia de cada fila
        _nombresDeEstado.Clear();

        foreach (var estado in _estados)
        {
            _nombresDeEstado[estado.Value] = estado.Name ?? string.Empty;
        }

        PintarPildoras();
    }

    // La pildora marcada es la del estado elegido; la lista llega armada del backend
    private void PintarPildoras()
    {
        Statuses = new ObservableCollection<ServiceRequestStatusChip>(
            _estados.Select(x => new ServiceRequestStatusChip(x, StatusFilter)));
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ServiceRequestSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Summary = response.Response;
    }
}

// Una pildora de filtro, ya resuelta: su texto, su valor y si esta puesta.
public class ServiceRequestStatusChip
{
    public string? Text { get; }

    //El chip compartido entrega el valor como texto en el CommandParameter
    public string Value { get; }

    public bool IsOn { get; }

    public ServiceRequestStatusChip(IntItemModel estado, int elegido)
    {
        Text = estado.Name;
        Value = estado.Value.ToString();
        IsOn = estado.Value == elegido;
    }
}
