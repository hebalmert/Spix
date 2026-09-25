using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedComponents.SharedCalendar;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule;

// Carga los eventos del calendario y conserva las mismas reglas de apertura de Blazor.
public partial class ScheduleIndexViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private List<ScheduleItemDto> _scheduleItems = new();

    [ObservableProperty]
    private ObservableCollection<CalendarEventModel> _events = new();

    //La lista del filtro la arma el backend, con "Todos" en la posicion 0
    [ObservableProperty]
    private ObservableCollection<IntItemModel> _statuses = new();

    //Que significa cada color del calendario
    [ObservableProperty]
    private ObservableCollection<ScheduleLegendItem> _legend = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool HasLegend => Legend.Count > 0;

    private int _statusFilter;

    // Cero es "todos": el calendario vuelve a mostrar la agenda completa
    public int StatusFilter
    {
        get => _statusFilter;
        set
        {
            if (_statusFilter == value)
            {
                return;
            }

            _statusFilter = value;
            OnPropertyChanged();

            AplicarFiltro();
        }
    }

    public ScheduleIndexViewModel(
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

    public event EventHandler? EventsChanged;

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    // El filtro y la leyenda salen de la MISMA lista que arma el backend: no se inventa
    // ningun estado ni se traduce nada aqui.
    public async Task LoadStatusesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>(
            "api/v1/schedulecontrol/loadStatusFilter");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<IntItemModel>();

        Statuses = new ObservableCollection<IntItemModel>(lista);

        //En la leyenda no va el "Todos" de la posicion 0: no es un estado, es el filtro
        Legend = new ObservableCollection<ScheduleLegendItem>(
            lista.Where(x => x.Value > 0).Select(x => new ScheduleLegendItem(x)));

        OnPropertyChanged(nameof(HasLegend));
    }

    // Con un estatus puesto solo quedan las citas de ese estatus. Se filtra sobre lo que
    // ya se bajo, como en la web: no se vuelve a pedir nada al servidor.
    private void AplicarFiltro()
    {
        var items = StatusFilter == 0
            ? _scheduleItems
            : _scheduleItems.Where(x => (int?)x.ScheduleStatus == StatusFilter).ToList();

        Events = new ObservableCollection<CalendarEventModel>(items.Select(CreateCalendarEvent));

        EventsChanged?.Invoke(this, EventArgs.Empty);
    }

    // Replica el rango usado por Blazor: un mes anterior y dos meses posteriores.
    public async Task LoadAsync()
    {
        IsLoading = true;
        Message = string.Empty;

        try
        {
            DateTime fromUtc = DateTime.UtcNow.AddMonths(-1);
            DateTime toUtc = DateTime.UtcNow.AddMonths(2);
            string url = $"api/v1/schedulecontrol?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O"))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O"))}";

            var response = await _repository.GetAsync<IEnumerable<ScheduleItemDto>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                _scheduleItems = new List<ScheduleItemDto>();
                Events = new ObservableCollection<CalendarEventModel>();
                EventsChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            _scheduleItems = response.Response?.ToList() ?? new List<ScheduleItemDto>();

            //Respeta el filtro que este puesto
            AplicarFiltro();
        }
        catch (Exception exception)
        {
            _scheduleItems = new List<ScheduleItemDto>();
            Events = new ObservableCollection<CalendarEventModel>();
            Message = "No fue posible cargar la agenda.";
            EventsChanged?.Invoke(this, EventArgs.Empty);
            await _alertService.ErrorAsync("Agenda", exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Abre la creacion usando la fecha elegida directamente desde FullCalendar.
    public async Task OpenCreateAsync(string selectedDate)
    {
        var parameters = new Dictionary<string, object>
        {
            ["SelectedDate"] = selectedDate
        };

        ModalResult result = await _modalService.ShowAsync<CreateScheduleDialogView>(
            "Crear agenda",
            parameters);

        if (result.Succeeded)
        {
            await LoadAsync();
            await _alertService.SuccessAsync("Guardado", "La agenda fue guardada correctamente.");
        }
    }

    // Respeta el origen de la agenda: solicitud de servicio solo se consulta desde Schedule.
    public async Task OpenEventAsync(string eventId)
    {
        if (!Guid.TryParse(eventId, out Guid id))
        {
            return;
        }

        ScheduleItemDto? item = _scheduleItems.FirstOrDefault(schedule => schedule.Id == id);
        if (item == null)
        {
            return;
        }

        if (item.Origin == ScheduleOrigin.ServiceRequest)
        {
            await _modalService.ShowAsync<ServiceRequestScheduleInfoDialogView>(
                "Solicitud de servicio",
                new Dictionary<string, object> { ["Id"] = id });
            return;
        }

        ModalResult result = await _modalService.ShowAsync<EditScheduleDialogView>(
            "Editar agenda",
            new Dictionary<string, object> { ["Id"] = id });

        if (result.Succeeded)
        {
            await LoadAsync();
            await _alertService.SuccessAsync("Actualizado", "La agenda fue actualizada correctamente.");
        }
    }

    // Cada estado con SU color, igual que el ScheduleColors de la web. Antes solo se
    // pintaban las citas cerradas y todas las demas salian del mismo azul, asi que el
    // calendario no decia nada de un golpe de vista.
    private CalendarEventModel CreateCalendarEvent(ScheduleItemDto item)
    {
        string color = ScheduleColors.Hex(item.ScheduleStatus);

        return new CalendarEventModel
        {
            Id = item.Id.ToString(),
            Title = $"{item.Title} - {item.TechnicianName}",
            Start = DateTime.SpecifyKind(item.StartUtc, DateTimeKind.Utc).ToLocalTime(),
            End = DateTime.SpecifyKind(item.EndUtc, DateTimeKind.Utc).ToLocalTime(),
            AllDay = item.IsAllDay,
            Origin = item.Origin,
            ServiceRequestId = item.ServiceRequestId,
            Color = color,
            TextColor = "#FFFFFF"
        };
    }
}

// Centraliza el formulario compartido por crear y editar agendas.
public abstract partial class ScheduleFormViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ScheduleItemDto _entity = new();

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _technicians = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _statuses = new();

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private string _startTime = "00:00";

    [ObservableProperty]
    private string _endTime = "01:00";

    [ObservableProperty]
    private int _selectedStatusValue = (int)ScheduleStatus.Pending;

    [ObservableProperty]
    private bool _isLoading;

    protected ScheduleFormViewModel(
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

    // Mantiene el enum del DTO sincronizado con el select nativo de WPF.
    partial void OnSelectedStatusValueChanged(int value)
    {
        if (Enum.IsDefined(typeof(ScheduleStatus), value))
        {
            Entity.ScheduleStatus = (ScheduleStatus)value;
        }
    }

    // Inicializa el nuevo registro tal como Blazor usa la fecha seleccionada del calendario.
    public async Task InitializeForCreateAsync(DateTime selectedLocalDate)
    {
        Entity = new ScheduleItemDto
        {
            ScheduleStatus = ScheduleStatus.Pending
        };

        StartDate = selectedLocalDate.Date;
        EndDate = selectedLocalDate.Date;
        StartTime = "00:00";
        EndTime = "01:00";
        SelectedStatusValue = (int)ScheduleStatus.Pending;
        await LoadCombosAsync();
    }

    // Carga agenda, tecnicos y estados antes de habilitar la edicion.
    public async Task InitializeForEditAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<ScheduleItemDto>($"api/v1/schedulecontrol/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Entity = response.Response ?? new ScheduleItemDto();
            SyncLocalDateTimeFields();
            await LoadCombosAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Guarda la agenda usando los mismos endpoints POST y PUT de Blazor.
    protected async Task SaveChangesAsync(bool isEdit)
    {
        if (!TryApplyLocalDateTimes(out string? validationMessage))
        {
            await _alertService.WarningAsync("Validacion", validationMessage!);
            return;
        }

        IsLoading = true;

        try
        {
            var response = isEdit
                ? await _repository.PutAsync($"api/v1/schedulecontrol/{Entity.Id}", Entity)
                : await _repository.PostAsync("api/v1/schedulecontrol", Entity);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Eliminar solo se habilita para agendas creadas desde el modulo Schedule.
    protected async Task DeleteScheduleAsync()
    {
        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar agenda",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.DeleteAsync($"api/v1/schedulecontrol/{Entity.Id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    // Consulta los mismos combos que el formulario FormSchedule de Blazor.
    private async Task LoadCombosAsync()
    {
        var statusResponse = await _repository.GetAsync<List<IntItemModel>>(
            "api/v1/schedulecontrol/loadStatus");
        if (!await _responseHandler.HandleErrorAsync(statusResponse))
        {
            Statuses = new ObservableCollection<IntItemModel>(
                statusResponse.Response ?? new List<IntItemModel>());
        }

        var technicianResponse = await _repository.GetAsync<IEnumerable<GuidItemModel>>(
            "api/v1/combosData/ComboTechnicians");
        if (!await _responseHandler.HandleErrorAsync(technicianResponse))
        {
            Technicians = new ObservableCollection<GuidItemModel>(
                technicianResponse.Response ?? Enumerable.Empty<GuidItemModel>());
        }
    }

    // Convierte las fechas UTC del Backend a los valores locales visibles en WPF.
    private void SyncLocalDateTimeFields()
    {
        DateTime startLocal = DateTime.SpecifyKind(Entity.StartUtc, DateTimeKind.Utc).ToLocalTime();
        DateTime endLocal = DateTime.SpecifyKind(Entity.EndUtc, DateTimeKind.Utc).ToLocalTime();

        StartDate = startLocal.Date;
        EndDate = endLocal.Date;
        StartTime = startLocal.ToString("HH:mm", CultureInfo.InvariantCulture);
        EndTime = endLocal.ToString("HH:mm", CultureInfo.InvariantCulture);
        SelectedStatusValue = (int)(Entity.ScheduleStatus ?? ScheduleStatus.Pending);
    }

    // Convierte los controles locales a UTC antes de llamar al Backend.
    private bool TryApplyLocalDateTimes(out string? validationMessage)
    {
        if (string.IsNullOrWhiteSpace(Entity.Title))
        {
            validationMessage = "Debes ingresar un titulo para la agenda.";
            return false;
        }

        if (Entity.TechnicianId == Guid.Empty)
        {
            validationMessage = "Debes seleccionar un tecnico valido.";
            return false;
        }

        if (!StartDate.HasValue || !EndDate.HasValue ||
            !TimeSpan.TryParse(StartTime, CultureInfo.InvariantCulture, out TimeSpan startTime) ||
            !TimeSpan.TryParse(EndTime, CultureInfo.InvariantCulture, out TimeSpan endTime))
        {
            validationMessage = "Debes seleccionar fechas y horas validas.";
            return false;
        }

        DateTime startLocal = StartDate.Value.Date.Add(startTime);
        DateTime endLocal = EndDate.Value.Date.Add(endTime);

        if (endLocal <= startLocal)
        {
            validationMessage = "La fecha final debe ser mayor que la fecha inicial.";
            return false;
        }

        Entity.StartUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
        Entity.EndUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();
        Entity.ScheduleStatus = (ScheduleStatus)SelectedStatusValue;
        validationMessage = null;
        return true;
    }
}

// Crea agendas propias desde una fecha seleccionada en FullCalendar.
public partial class CreateScheduleDialogViewModel : ScheduleFormViewModel
{
    public CreateScheduleDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

// Edita o elimina agendas propias conservando la proteccion para solicitudes de servicio.
public partial class EditScheduleDialogViewModel : ScheduleFormViewModel
{
    public EditScheduleDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await base.DeleteScheduleAsync();
    }
}

// Consulta una agenda originada por Solicitud de Servicio sin permitir alterarla desde Schedule.
public partial class ServiceRequestScheduleInfoDialogViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly NavigationService _navigationService;

    [ObservableProperty]
    private ScheduleItemDto? _schedule;

    [ObservableProperty]
    private bool _isLoading;

    public string StatusName => Schedule?.ScheduleStatus?.ToString() ?? string.Empty;

    public DateTime? StartLocal => Schedule == null
        ? null
        : DateTime.SpecifyKind(Schedule.StartUtc, DateTimeKind.Utc).ToLocalTime();

    public DateTime? EndLocal => Schedule == null
        ? null
        : DateTime.SpecifyKind(Schedule.EndUtc, DateTimeKind.Utc).ToLocalTime();

    // Desde el calendario se puede ir a la orden de trabajo de la solicitud
    public bool HasServiceRequest => Schedule?.ServiceRequestId is not null &&
                                     Schedule.ServiceRequestId != Guid.Empty;

    public ServiceRequestScheduleInfoDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        NavigationService navigationService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _navigationService = navigationService;
    }

    // Cierra el aviso del calendario y abre la orden, igual que en la web
    [RelayCommand]
    private async Task GoToServiceRequestAsync()
    {
        if (!HasServiceRequest)
        {
            return;
        }

        var id = Schedule!.ServiceRequestId!.Value;

        await _modalService.CloseAsync(ModalResult.Ok());

        _navigationService.Show<Views.EntitiesSchedule.ServiceRequest.ServiceRequestOrderView>(
            "Orden de trabajo",
            "Operaciones / Solicitud de servicio",
            vista => vista.Prepare(id));
    }

    // Recupera los datos de solo lectura que Blazor muestra para una solicitud de servicio.
    public async Task InitializeAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<ScheduleItemDto>($"api/v1/schedulecontrol/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Schedule = response.Response;
            OnPropertyChanged(nameof(StatusName));
            OnPropertyChanged(nameof(StartLocal));
            OnPropertyChanged(nameof(EndLocal));
            OnPropertyChanged(nameof(HasServiceRequest));
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

// Un color de la leyenda del calendario, ya resuelto: su nombre y su pincel.
public class ScheduleLegendItem
{
    public string? Name { get; }

    public System.Windows.Media.Brush Color { get; }

    public ScheduleLegendItem(IntItemModel estado)
    {
        Name = estado.Name;
        Color = ScheduleColors.Pincel(estado.Value);
    }
}
