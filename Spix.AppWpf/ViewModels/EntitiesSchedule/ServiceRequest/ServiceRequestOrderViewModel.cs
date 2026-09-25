using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;

// La orden de trabajo, replicada del FormServiceRequest de la web en modo edicion.
//
// No es un formulario: es un RECORRIDO. Registrada -> En sitio -> Cerrada, con un solo
// boton grande para que el tecnico no tenga que adivinar que estado cierra la visita.
//
// Y una regla dura que viene de la web: NO se cierra sin un servicio cargado, sin
// comentario del tecnico y sin foto del despues. Por eso la pantalla muestra lo que falta.
public partial class ServiceRequestOrderViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/servicerequests";
    private const string DetailUrl = "api/v1/servicerequestdetails";
    private const string PhotoUrl = "api/v1/servicerequestphotos";
    private const string TechniciansUrl = "/api/v1/combosData/ComboTechnicians";
    private const string StatusUrl = "/api/v1/schedulecontrol/loadStatusChange";
    private const string CategoriesUrl = "/api/v1/combosData/ComboServiceCategories";

    //La visita admite cuatro fotos, como en la web
    private const int MaximoDeFotos = 4;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ServiceRequestDto? _entity;

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _technicians = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _statuses = new();

    [ObservableProperty]
    private ObservableCollection<ServiceCategory> _categories = new();

    [ObservableProperty]
    private ObservableCollection<ServiceClient> _services = new();

    [ObservableProperty]
    private ObservableCollection<ServiceRequestPhotoRow> _photos = new();

    [ObservableProperty]
    private Guid _selectedCategoryId;

    [ObservableProperty]
    private Guid _selectedServiceId;

    [ObservableProperty]
    private string _detailText = string.Empty;

    [ObservableProperty]
    private DateTime? _scheduledDate;

    [ObservableProperty]
    private string _scheduledTime = "08:00";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSavingStep;

    private Guid _id;

    // Se pide al salir de la orden
    public event EventHandler? BackRequested;

    public ServiceRequestOrderViewModel(
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

    //===================== Como va la orden =====================

    // La pidio el cliente y nadie la ha revisado todavia: primero hay que triarla
    public bool IsRequested => Entity?.ScheduleStatus == ScheduleStatus.Requested;

    public bool IsCompleted =>
        Entity?.ScheduleStatus == ScheduleStatus.Completed ||
        Entity?.ScheduleStatus == ScheduleStatus.PhoneResolved;

    public bool IsStarted =>
        Entity is not null &&
        Entity.ScheduleStatus != ScheduleStatus.Pending &&
        Entity.ScheduleStatus != ScheduleStatus.Requested;

    // Solo con la visita arrancada se cargan servicios, fotos y el cierre del tecnico
    public bool CanWork => IsStarted && !IsCompleted;

    public bool ShowSteps => Entity is not null && !IsRequested;

    // El boton grande solo aparece mientras haya algo que hacer
    public bool ShowAdvance => Entity is not null && !IsRequested && !IsCompleted;

    public string AdvanceText => IsStarted ? "Cerrar la visita" : "Iniciar la visita";

    //===================== Lo que hace falta para cerrar =====================

    public bool HasService => Entity is not null && Entity.Details.Count > 0;

    public bool HasComment => !string.IsNullOrWhiteSpace(Entity?.TechnicianComment);

    public bool HasAfterPhoto => Entity?.HasPhotoAfter == true;

    public bool CanClose => HasService && HasComment && HasAfterPhoto;

    // Se muestra la lista de lo que falta solo mientras falte algo
    public bool ShowPending => CanWork && !CanClose;

    public bool CanAddPhoto => CanWork && Photos.Count < MaximoDeFotos;

    public bool HasEntity => Entity is not null;

    public string Header => Entity is null
        ? string.Empty
        : $"Orden de trabajo #{Entity.RequestNumber} · {Entity.ClientFullName} · Contrato {Entity.ControlContrato}";

    public string SubHeader => Entity is null
        ? string.Empty
        : $"{Entity.Address} · {Entity.CityName} / {Entity.ZoneName}";

    // El telefono de contacto puede ser otro: el cliente pidio que lo llamaran ahi
    public string? ContactPhone => string.IsNullOrWhiteSpace(Entity?.ContactPhone)
        ? Entity?.PhoneNumber
        : Entity?.ContactPhone;

    public async Task InitializeAsync(Guid id)
    {
        _id = id;

        IsLoading = true;

        try
        {
            await LoadTechniciansAsync();
            await LoadStatusesAsync();
            await LoadCategoriesAsync();
            await RecargarAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    //===================== El boton grande =====================

    [RelayCommand]
    private async Task AdvanceAsync()
    {
        if (Entity is null)
        {
            return;
        }

        //Arrancar la visita solo guarda el estado: el tecnico se queda trabajando aqui
        if (!IsStarted)
        {
            await IniciarAsync();
            return;
        }

        if (!CanClose)
        {
            await _alertService.WarningAsync(
                "Cerrar la visita",
                "Para cerrar hace falta al menos un servicio cargado, el comentario del tecnico y una foto del despues.");
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Cerrar la visita",
            "Se cierra la orden y ya no se podra modificar. Desea continuar?",
            "Cerrar");

        if (!confirmado)
        {
            return;
        }

        IsSavingStep = true;

        try
        {
            //Cerrar tiene su propio endpoint: de paso guarda lo que el tecnico escribio
            var url = $"{BaseUrl}/{_id}/close" +
                      $"?comment={Uri.EscapeDataString(Entity.TechnicianComment ?? string.Empty)}" +
                      $"&recommendation={Uri.EscapeDataString(Entity.Recommendation ?? string.Empty)}";

            var response = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            //Cerrada la orden ya no hay nada mas que hacer aqui
            BackRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsSavingStep = false;
        }
    }

    private async Task IniciarAsync()
    {
        var anterior = Entity!.ScheduleStatus;

        Entity.ScheduleStatus = ScheduleStatus.InProgress;

        IsSavingStep = true;

        try
        {
            var response = await _repository.PutAsync(BaseUrl, Entity);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                Entity.ScheduleStatus = anterior;
                return;
            }

            await RecargarAsync();
        }
        finally
        {
            IsSavingStep = false;
        }
    }

    //===================== La tarjeta de la visita =====================

    // Guarda SOLO esta tarjeta: el recorrido, los servicios y las fotos se guardan solos
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Entity is null)
        {
            return;
        }

        if (!TryArmarFecha(out var mensaje) || !TryValidar(out mensaje))
        {
            await _alertService.WarningAsync("Validacion", mensaje!);
            return;
        }

        IsSaving = true;

        try
        {
            var response = await _repository.PutAsync(BaseUrl, Entity);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Actualizada", "La orden fue actualizada correctamente.");

            await RecargarAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    //===================== Servicios realizados =====================

    // Al elegir categoria se bajan sus servicios, como en la web
    partial void OnSelectedCategoryIdChanged(Guid value)
    {
        _ = LoadServicesAsync(value);
    }

    private async Task LoadServicesAsync(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            Services = new ObservableCollection<ServiceClient>();
            return;
        }

        var response = await _repository.GetAsync<List<ServiceClient>>($"/api/v1/combosData/ComboServiceClients/{categoryId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Services = new ObservableCollection<ServiceClient>(response.Response ?? new List<ServiceClient>());
    }

    [RelayCommand]
    private async Task AddDetailAsync()
    {
        if (Entity is null || SelectedServiceId == Guid.Empty)
        {
            await _alertService.WarningAsync("Servicios", "Debe elegir la categoria y el servicio.");
            return;
        }

        var detalle = new ServiceRequestDetailDto
        {
            ServiceRequestId = _id,
            ServiceCategoryId = SelectedCategoryId,
            ServiceClientId = SelectedServiceId,
            Detail = DetailText
        };

        var response = await _repository.PostAsync(DetailUrl, detalle);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        SelectedServiceId = Guid.Empty;
        DetailText = string.Empty;
        Services = new ObservableCollection<ServiceClient>();
        SelectedCategoryId = Guid.Empty;

        await RecargarAsync();
    }

    [RelayCommand]
    private async Task DeleteDetailAsync(ServiceRequestDetailDto? detalle)
    {
        if (detalle is null)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{DetailUrl}/{detalle.ServiceRequestDetailId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();
    }

    //===================== Fotos de la visita =====================
    // Son de la VISITA, no de cada servicio

    [RelayCommand]
    private async Task AddPhotoAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["ServiceRequestId"] = _id
        };

        var result = await _modalService.ShowAsync<Views.EntitiesSchedule.ServiceRequest.UploadServicePhotoDialogView>(
            "Foto de la visita", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // Tocar una miniatura abre el carrusel
    [RelayCommand]
    private async Task ViewPhotoAsync(ServiceRequestPhotoRow? foto)
    {
        if (foto is null || Entity is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Photos"] = Entity.Photos,
            ["StartId"] = foto.Item.ServiceRequestPhotoId
        };

        await _modalService.ShowAsync<Views.EntitiesSchedule.ServiceRequest.ServicePhotoViewerDialogView>(
            "Fotos de la visita", parametros);
    }

    [RelayCommand]
    private async Task DeletePhotoAsync(ServiceRequestPhotoRow? foto)
    {
        if (foto is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar foto",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{PhotoUrl}/{foto.Item.ServiceRequestPhotoId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();
    }

    //===================== Triaje de lo que pide el cliente =====================

    // Agendar: se elige tecnico y fecha, y la solicitud pasa a ser una visita
    [RelayCommand]
    private async Task AssignAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["ServiceRequestId"] = _id
        };

        var result = await _modalService.ShowAsync<Views.EntitiesSchedule.ServiceRequest.AssignServiceRequestDialogView>(
            "Agendar la visita", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // Resolver por telefono: su propio modal, porque lo que se escriba lo lee el cliente
    [RelayCommand]
    private async Task ResolveByPhoneAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["ServiceRequestId"] = _id
        };

        var result = await _modalService.ShowAsync<Views.EntitiesSchedule.ServiceRequest.ResolveByPhoneDialogView>(
            "Resolver por telefono", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    //===================== Carga =====================

    // Vuelve a pedir la orden completa: los enlaces de las fotos duran poco y, con el
    // estado, cambia todo lo que se puede hacer en la pantalla.
    private async Task RecargarAsync()
    {
        var response = await _repository.GetAsync<ServiceRequestDto>($"{BaseUrl}/{_id}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Entity = response.Response;

        if (Entity?.ScheduledAtUtc is not null)
        {
            var local = DateTime.SpecifyKind(Entity.ScheduledAtUtc.Value, DateTimeKind.Utc).ToLocalTime();

            ScheduledDate = local.Date;
            ScheduledTime = local.ToString("HH:mm");
        }

        Photos = new ObservableCollection<ServiceRequestPhotoRow>(
            (Entity?.Photos ?? new List<ServiceRequestPhotoDto>()).Select(x => new ServiceRequestPhotoRow(x)));

        RefrescarEstado();
    }

    // Un solo sitio para avisar de todo lo que depende del estado de la orden
    private void RefrescarEstado()
    {
        OnPropertyChanged(nameof(HasEntity));
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(SubHeader));
        OnPropertyChanged(nameof(ContactPhone));
        OnPropertyChanged(nameof(IsRequested));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(CanWork));
        OnPropertyChanged(nameof(ShowSteps));
        OnPropertyChanged(nameof(ShowAdvance));
        OnPropertyChanged(nameof(AdvanceText));
        OnPropertyChanged(nameof(HasService));
        OnPropertyChanged(nameof(HasComment));
        OnPropertyChanged(nameof(HasAfterPhoto));
        OnPropertyChanged(nameof(CanClose));
        OnPropertyChanged(nameof(ShowPending));
        OnPropertyChanged(nameof(CanAddPhoto));
    }

    private async Task LoadTechniciansAsync()
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>(TechniciansUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Technicians = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    private async Task LoadStatusesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>(StatusUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Statuses = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task LoadCategoriesAsync()
    {
        var response = await _repository.GetAsync<List<ServiceCategory>>(CategoriesUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Categories = new ObservableCollection<ServiceCategory>(response.Response ?? new List<ServiceCategory>());
    }

    // La fecha se escribe en hora local y viaja en UTC, como la guarda el sistema
    private bool TryArmarFecha(out string? mensaje)
    {
        mensaje = null;

        if (ScheduledDate is null)
        {
            mensaje = "Debe indicar la fecha programada.";
            return false;
        }

        if (!TimeSpan.TryParseExact(ScheduledTime?.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var hora))
        {
            mensaje = "La hora programada debe tener el formato HH:mm.";
            return false;
        }

        var local = ScheduledDate.Value.Date.Add(hora);

        Entity!.ScheduledAtUtc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

        return true;
    }

    // Las mismas validaciones de la web, en el mismo orden
    private bool TryValidar(out string? mensaje)
    {
        if (Entity!.TechnicianId is null || Entity.TechnicianId == Guid.Empty)
        {
            mensaje = "Debe seleccionar un tecnico activo.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity.ClientReason))
        {
            mensaje = "Debe indicar la razon de la llamada.";
            return false;
        }

        mensaje = null;
        return true;
    }
}

// Una foto de la visita, ya lista para pintar
public class ServiceRequestPhotoRow
{
    public ServiceRequestPhotoDto Item { get; }

    public string? ImageFullPath => Item.ImageFullPath;

    public bool IsAfter => Item.PhotoType == ServicePhotoType.After;

    public string Tag => IsAfter ? "Despues" : "Antes";

    public ServiceRequestPhotoRow(ServiceRequestPhotoDto item)
    {
        Item = item;
    }
}
