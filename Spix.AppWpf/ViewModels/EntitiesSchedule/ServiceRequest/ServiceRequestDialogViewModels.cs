using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;

// Agendar la visita: le pone tecnico y fecha a una solicitud que pidio el cliente.
// Recien ahi nace la cita, igual que el AssignServiceRequest de la web.
public partial class AssignServiceRequestDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/servicerequests";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _technicians = new();

    [ObservableProperty]
    private Guid _technicianId;

    [ObservableProperty]
    private DateTime? _scheduledDate = DateTime.Now.AddDays(1).Date;

    [ObservableProperty]
    private string _scheduledTime = "08:00";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _serviceRequestId;

    public AssignServiceRequestDialogViewModel(
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

    public async Task InitializeAsync(Guid serviceRequestId)
    {
        _serviceRequestId = serviceRequestId;

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<GuidItemModel>>("/api/v1/combosData/ComboTechnicians");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Technicians = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (TechnicianId == Guid.Empty)
        {
            await _alertService.WarningAsync("Agendar la visita", "Debe seleccionar un tecnico activo.");
            return;
        }

        if (!TryArmarFecha(out var local, out var mensaje))
        {
            await _alertService.WarningAsync("Agendar la visita", mensaje!);
            return;
        }

        IsSaving = true;

        try
        {
            //La fecha viaja en UTC, como la guarda el sistema
            var url = $"{BaseUrl}/{_serviceRequestId}/assign" +
                      $"?technicianId={TechnicianId}&scheduledAtUtc={local.ToUniversalTime():O}";

            var response = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });
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

    private bool TryArmarFecha(out DateTime local, out string? mensaje)
    {
        local = default;
        mensaje = null;

        if (ScheduledDate is null)
        {
            mensaje = "Debe indicar la fecha programada.";
            return false;
        }

        if (!TimeSpan.TryParseExact(ScheduledTime?.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var hora))
        {
            mensaje = "La hora debe tener el formato HH:mm.";
            return false;
        }

        local = DateTime.SpecifyKind(ScheduledDate.Value.Date.Add(hora), DateTimeKind.Local);

        return true;
    }
}

// Cierra la solicitud sin visita.
// Lo que se escribe aqui es lo UNICO que el cliente va a ver de la resolucion, por eso
// son dos campos separados y no un texto suelto.
public partial class ResolveByPhoneDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/servicerequests";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private string _recommendation = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _serviceRequestId;

    public ResolveByPhoneDialogViewModel(
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

    public void Initialize(Guid serviceRequestId)
    {
        _serviceRequestId = serviceRequestId;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Comment))
        {
            await _alertService.WarningAsync(
                "Resolver por telefono",
                "Debe explicar como se resolvio: es lo que el cliente va a leer.");
            return;
        }

        IsSaving = true;

        try
        {
            var url = $"{BaseUrl}/{_serviceRequestId}/resolvebyphone" +
                      $"?comment={Uri.EscapeDataString(Comment.Trim())}" +
                      $"&recommendation={Uri.EscapeDataString(Recommendation?.Trim() ?? string.Empty)}";

            var response = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });
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

// Sube UNA foto de la visita. La orden se entera al cerrarse el modal y recarga su lista.
// La foto sale del disco o de la camara: es el mismo selector del cliente.
public partial class UploadServicePhotoDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/servicerequestphotos";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private bool _isAfter;

    [ObservableProperty]
    private bool _isSaving;

    private string? _base64;
    private Guid _serviceRequestId;

    public UploadServicePhotoDialogViewModel(
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

    public void Initialize(Guid serviceRequestId)
    {
        _serviceRequestId = serviceRequestId;
    }

    // La entrega el selector, ya en el Base64 que espera el Backend
    public void SetPhoto(string base64)
    {
        _base64 = base64;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_base64))
        {
            await _alertService.WarningAsync("Foto de la visita", "Debe tomar o elegir una foto.");
            return;
        }

        var dto = new ServiceRequestPhotoDto
        {
            ServiceRequestId = _serviceRequestId,
            PhotoType = IsAfter ? ServicePhotoType.After : ServicePhotoType.Before,
            ImgBase64 = _base64
        };

        IsSaving = true;

        try
        {
            var response = await _repository.PostAsync<ServiceRequestPhotoDto, ServiceRequestPhotoDto>(BaseUrl, dto);
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
