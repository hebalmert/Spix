using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;

// Registrar una solicitud de servicio, igual que el CreateServiceRequest de la web.
//
// Son cuatro cosas: de quien es (el contrato), quien va (el tecnico), para cuando y por
// que. Lo demas —servicios, fotos y el cierre— vive en la orden de trabajo, no aqui.
public partial class CreateServiceRequestDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/servicerequests";
    private const string TechniciansUrl = "/api/v1/combosData/ComboTechnicians";

    //Desde dos letras: con una sola la busqueda traeria medio archivo
    private const int MinimoParaBuscar = 2;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ServiceRequestDto _entity = new()
    {
        ScheduleStatus = ScheduleStatus.Pending
    };

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _technicians = new();

    //Lo que va cayendo mientras se escribe el cliente o el contrato
    [ObservableProperty]
    private ObservableCollection<ServiceRequestContractDto> _contracts = new();

    [ObservableProperty]
    private string _contractFilter = string.Empty;

    [ObservableProperty]
    private DateTime? _scheduledDate = DateTime.Now.Date;

    [ObservableProperty]
    private string _scheduledTime = DateTime.Now.AddHours(1).ToString("HH:00");

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    // Con el contrato elegido se muestra su tarjeta de datos, como en la web
    public bool HasContract => Entity.ContractClientId != Guid.Empty;

    public CreateServiceRequestDialogViewModel(
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

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<GuidItemModel>>(TechniciansUrl);
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

    // Se busca desde dos letras; con menos se limpia lo que hubiera
    [RelayCommand]
    private async Task SearchContractsAsync(string? texto)
    {
        ContractFilter = texto ?? string.Empty;

        if (ContractFilter.Trim().Length < MinimoParaBuscar)
        {
            Contracts = new ObservableCollection<ServiceRequestContractDto>();
            return;
        }

        var url = $"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(ContractFilter.Trim())}";

        var response = await _repository.GetAsync<List<ServiceRequestContractDto>>(url);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Contracts = new ObservableCollection<ServiceRequestContractDto>(
            response.Response ?? new List<ServiceRequestContractDto>());
    }

    // Al elegir el contrato se copian sus datos: son los que veria el tecnico en la visita
    [RelayCommand]
    private void SelectContract(ServiceRequestContractDto? contrato)
    {
        if (contrato is null)
        {
            return;
        }

        Entity.ContractClientId = contrato.ContractClientId;
        Entity.ControlContrato = contrato.ControlContrato;
        Entity.ClientFullName = contrato.ClientFullName;
        Entity.PhoneNumber = contrato.PhoneNumber;
        Entity.Address = contrato.Address;
        Entity.CityName = contrato.CityName;
        Entity.ZoneName = contrato.ZoneName;
        Entity.ServerName = contrato.ServerName;
        Entity.IpServer = contrato.IpServer;
        Entity.IpCliente = contrato.IpCliente;
        Entity.MacCliente = contrato.MacCliente;
        Entity.PlanName = contrato.PlanName;
        Entity.PlanSpeed = contrato.PlanSpeed;
        Entity.NodeName = contrato.NodeName;
        Entity.NodeIp = contrato.NodeIp;

        ContractFilter = $"{contrato.ClientFullName} - Contrato {contrato.ControlContrato}";

        //La lista se cierra sola al quedarse sin resultados
        Contracts = new ObservableCollection<ServiceRequestContractDto>();

        OnPropertyChanged(nameof(Entity));
        OnPropertyChanged(nameof(HasContract));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!TryArmarFecha(out var mensaje))
        {
            await _alertService.WarningAsync("Validacion", mensaje!);
            return;
        }

        if (!TryValidar(out mensaje))
        {
            await _alertService.WarningAsync("Validacion", mensaje!);
            return;
        }

        IsSaving = true;

        try
        {
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

        Entity.ScheduledAtUtc = DateTime.SpecifyKind(local, DateTimeKind.Local).ToUniversalTime();

        return true;
    }

    // Las mismas cuatro validaciones de la web, en el mismo orden
    private bool TryValidar(out string? mensaje)
    {
        if (Entity.ContractClientId == Guid.Empty)
        {
            mensaje = "Debe seleccionar un contrato activo.";
            return false;
        }

        if (Entity.TechnicianId is null || Entity.TechnicianId == Guid.Empty)
        {
            mensaje = "Debe seleccionar un tecnico activo.";
            return false;
        }

        if (Entity.ScheduledAtUtc is null || Entity.ScheduledAtUtc == default(DateTime))
        {
            mensaje = "Debe seleccionar fecha y hora programada.";
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
