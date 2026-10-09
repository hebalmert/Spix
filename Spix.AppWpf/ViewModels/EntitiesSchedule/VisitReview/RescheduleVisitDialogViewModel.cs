using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.VisitReview;

// Reagendar la visita en la que no estaba el cliente. El backend crea una NUEVA enlazada
// a la que fallo; la vieja no se mueve para no borrar la evidencia del intento.
// Calcado del AssignServiceRequestDialogViewModel, que ya resuelve fecha y hora.
public partial class RescheduleVisitDialogViewModel : ObservableObject
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

    //De que visita sale la nueva, para no reagendar la equivocada
    [ObservableProperty]
    private string _fromText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _serviceRequestId;

    public RescheduleVisitDialogViewModel(
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

    public async Task InitializeAsync(Guid serviceRequestId, long requestNumber, string? clientFullName)
    {
        _serviceRequestId = serviceRequestId;
        FromText = $"Visita #{requestNumber} · {clientFullName}";

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
            await _alertService.WarningAsync("Reagendar", "Debe seleccionar un tecnico activo.");
            return;
        }

        if (!TryArmarFecha(out var local, out var mensaje))
        {
            await _alertService.WarningAsync("Reagendar", mensaje!);
            return;
        }

        IsSaving = true;

        try
        {
            //La fecha viaja en UTC, como la guarda el sistema
            var url = $"{BaseUrl}/{_serviceRequestId}/reschedule" +
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
