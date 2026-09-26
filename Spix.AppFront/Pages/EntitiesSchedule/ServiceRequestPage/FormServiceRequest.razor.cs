using Spix.AppFront.GenericModel;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class FormServiceRequest
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter, EditorRequired] public ServiceRequestDto Model { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private const string BaseUrl = "/api/v1/servicerequests";

    //Cada responsabilidad tiene su controlador: la solicitud, lo que se cobra y las fotos
    private const string BaseDetailUrl = "/api/v1/servicerequestdetails";
    private const string BasePhotoUrl = "/api/v1/servicerequestphotos";
    private const string BaseView = "/servicerequests";
    private const string BaseComboStatus = "/api/v1/schedulecontrol/loadStatusChange";
    private List<ServiceRequestContractDto> Contracts = new();
    private List<GuidItemModel>? Technicians = new();
    private List<IntItemModel>? ScheduleStatuses;
    private List<ServiceCategory> ServiceCategories = new();
    private List<ServiceClient> ServiceClients = new();
    private ServiceRequestDetailDto Detail = new();
    private string ContractFilter = string.Empty;
    private bool IsCompleted;
    private bool IsSavingStep;


    protected override async Task OnInitializedAsync()
    {
        await LoadTechnicians();
        await LoadStatus();
        await LoadServiceCategories();

        if (IsEditControl)
        {
            //Cerrada son las dos: completada en sitio y resuelta por telefono. Con
            //solo Completed, una resuelta por telefono se abria editable.
            IsCompleted = Model.ScheduleStatus.IsClosed();
            Detail = new() { ServiceRequestId = Model.ServiceRequestId };
        }
    }

    private async Task LoadTechnicians()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>("/api/v1/combosData/ComboTechnicians");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo(BaseView);
            return;
        }

        Technicians = responseHttp.Response ?? new();
    }

    private async Task LoadStatus()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>(BaseComboStatus);
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo(BaseView);
            return;
        }

        ScheduleStatuses = responseHttp.Response ?? new();
    }

    private async Task LoadServiceCategories()
    {
        var responseHttp = await _repository.GetAsync<List<ServiceCategory>>("/api/v1/combosData/ComboServiceCategories");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo(BaseView);
            return;
        }

        ServiceCategories = responseHttp.Response ?? new();
    }

    private async Task SearchContracts()
    {
        if (ContractFilter.Length < 2)
        {
            Contracts.Clear();
            return;
        }

        var responseHttp = await _repository.GetAsync<List<ServiceRequestContractDto>>($"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(ContractFilter)}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Contracts = responseHttp.Response ?? new();
        }
    }

    private void SelectContract(ServiceRequestContractDto contract)
    {
        Model.ContractClientId = contract.ContractClientId;
        Model.ControlContrato = contract.ControlContrato;
        Model.ClientFullName = contract.ClientFullName;
        Model.PhoneNumber = contract.PhoneNumber;
        Model.Address = contract.Address;
        Model.CityName = contract.CityName;
        Model.ZoneName = contract.ZoneName;
        Model.ServerName = contract.ServerName;
        Model.IpServer = contract.IpServer;
        Model.IpCliente = contract.IpCliente;
        Model.MacCliente = contract.MacCliente;
        Model.PlanName = contract.PlanName;
        Model.PlanSpeed = contract.PlanSpeed;
        Model.NodeName = contract.NodeName;
        Model.NodeIp = contract.NodeIp;

        ContractFilter = $"{contract.ClientFullName} - Contrato {contract.ControlContrato}";
        Contracts.Clear();
    }

    private void TechnicianChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var technicianId))
        {
            Model.TechnicianId = technicianId;
        }
    }

    private void StatusChanged(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var value) || value == 0)
            return;

        Model.ScheduleStatus = (ScheduleStatus)value;
    }

    private void OnScheduledChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var dt))
        {
            Model.ScheduledAtUtc = DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
        }
    }

    private async Task CategoryChanged(ChangeEventArgs e)
    {
        if (!Guid.TryParse(e.Value?.ToString(), out var categoryId) || categoryId == Guid.Empty)
        {
            ServiceClients.Clear();
            return;
        }

        Detail.ServiceCategoryId = categoryId;
        var responseHttp = await _repository.GetAsync<List<ServiceClient>>($"/api/v1/combosData/ComboServiceClients/{categoryId}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            ServiceClients = responseHttp.Response ?? new();
        }
    }

    private void ServiceClientChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var serviceClientId))
        {
            Detail.ServiceClientId = serviceClientId;
        }
    }

    private async Task AddDetail()
    {
        Detail.ServiceRequestId = Model.ServiceRequestId;
        var responseHttp = await _repository.PostAsync(BaseDetailUrl, Detail);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await ReloadRequest();
        Detail = new() { ServiceRequestId = Model.ServiceRequestId };
        ServiceClients.Clear();
    }

    private async Task DeleteDetail(Guid id)
    {
        var responseHttp = await _repository.DeleteAsync($"{BaseDetailUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await ReloadRequest();
    }

    private async Task ReloadRequest()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestDto>($"{BaseUrl}/{Model.ServiceRequestId}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            var request = responseHttp.Response;
            if (request == null)
                return;

            Model.Details = request.Details;
            Model.Billed = request.Billed;
            Model.SellId = request.SellId;
            Model.SubTotal = request.SubTotal;
            Model.TotalTax = request.TotalTax;
            Model.Total = request.Total;
        }
    }

    //===================== Recorrido de la orden =====================
    //Registrada -> En sitio -> Cerrada. Un solo boton grande, para que el tecnico no
    //tenga que adivinar cual estatus cierra la visita.

    private bool IsStarted => Model.ScheduleStatus != ScheduleStatus.Pending &&
                              Model.ScheduleStatus != ScheduleStatus.Requested;

    //La pidio el cliente y nadie la ha revisado todavia
    private bool IsRequested => Model.ScheduleStatus == ScheduleStatus.Requested;

    //Solo con la visita arrancada se cargan servicios, fotos y el cierre del tecnico
    private bool CanWork => IsStarted && !IsCompleted;

    //Lo que hace falta para poder cerrar
    private bool HasService => Model.Details.Count > 0;

    private bool HasComment => !string.IsNullOrWhiteSpace(Model.TechnicianComment);

    private bool HasAfterPhoto => Model.HasPhotoAfter;

    private bool CanClose => HasService && HasComment && HasAfterPhoto;

    //El boton principal: arranca la visita o la cierra, segun donde va la orden
    private async Task AdvanceAsync()
    {
        //Arrancar la visita solo guarda el estado: el tecnico se queda en la orden
        //trabajando, por eso aqui no se cierra el modal.
        if (!IsStarted)
        {
            Model.ScheduleStatus = ScheduleStatus.InProgress;

            IsSavingStep = true;
            var responseHttp = await _repository.PutAsync(BaseUrl, Model);
            IsSavingStep = false;

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                Model.ScheduleStatus = ScheduleStatus.Pending;
                return;
            }

            await InvokeAsync(StateHasChanged);
            return;
        }

        if (!CanClose)
        {
            await _sweetAlert.FireAsync(
                Localizer["Close_Title"],
                Localizer["Close_Missing"],
                SweetAlertIcon.Warning);
            return;
        }

        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Close_Title"],
            Text = Localizer["Close_Question"],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Close_Button"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
            return;

        //Cerrar tiene su propio endpoint: de paso guarda lo que el tecnico escribio
        IsSavingStep = true;
        await InvokeAsync(StateHasChanged);

        var url = $"{BaseUrl}/{Model.ServiceRequestId}/close" +
                  $"?comment={Uri.EscapeDataString(Model.TechnicianComment ?? string.Empty)}" +
                  $"&recommendation={Uri.EscapeDataString(Model.Recommendation ?? string.Empty)}";

        var cierre = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });

        IsSavingStep = false;

        if (await _responseHandler.HandleErrorAsync(cierre))
            return;

        //Cerrada la orden ya no hay nada mas que hacer aqui
        await ReturnAction.InvokeAsync();
    }

    //===================== Fotos de la visita =====================
    //Subir una foto vive en su propio modal: se elige antes/despues y se trae de
    //la camara o del disco. Al cerrarse, la orden recarga sus fotos.
    private async Task ShowPhotoUploadAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "ServiceRequestId", Model.ServiceRequestId }
        };

        await _modalService.ShowAsync(typeof(UploadServicePhoto), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadPhotosAsync();
            }
        });
    }

    //Tocar una miniatura abre el carrusel
    private async Task ShowPhotoViewerAsync(ServiceRequestPhotoDto photo)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Photos", Model.Photos },
            { "StartId", photo.ServiceRequestPhotoId }
        };

        await _modalService.ShowAsync(typeof(ServicePhotoViewer), parameters);
    }

    //Los enlaces del blob duran poco, asi que se piden de nuevo con la orden
    private async Task ReloadPhotosAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestDto>($"{BaseUrl}/{Model.ServiceRequestId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Model.Photos = responseHttp.Response?.Photos ?? new();
        Model.HasPhotoBefore = responseHttp.Response?.HasPhotoBefore ?? false;
        Model.HasPhotoAfter = responseHttp.Response?.HasPhotoAfter ?? false;

        await InvokeAsync(StateHasChanged);
    }

    private async Task DeletePhotoAsync(ServiceRequestPhotoDto photo)
    {
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BasePhotoUrl}/{photo.ServiceRequestPhotoId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await ReloadPhotosAsync();
    }

    //===================== Triaje de lo que pide el cliente =====================
    //Agendar: se elige tecnico y fecha, y la solicitud pasa a ser una visita
    private async Task ShowAssignAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "ServiceRequestId", Model.ServiceRequestId }
        };

        await _modalService.ShowAsync(typeof(AssignServiceRequest), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync();
            }
        });
    }

    //Resolver por telefono: su propio modal, porque lo que se escriba lo lee el cliente
    private async Task ResolveByPhoneAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "ServiceRequestId", Model.ServiceRequestId }
        };

        await _modalService.ShowAsync(typeof(ResolveByPhone), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync();
            }
        });
    }

    //Vuelve a pedir la orden completa: cambio el estado y con el, lo que se puede hacer
    private async Task ReloadAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestDto>($"{BaseUrl}/{Model.ServiceRequestId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        if (responseHttp.Response is not null)
        {
            Model.ScheduleStatus = responseHttp.Response.ScheduleStatus;
            Model.TechnicianId = responseHttp.Response.TechnicianId;
            Model.ScheduledAtUtc = responseHttp.Response.ScheduledAtUtc;
            Model.TechnicianComment = responseHttp.Response.TechnicianComment;
            Model.Recommendation = responseHttp.Response.Recommendation;
        }

        IsCompleted = Model.ScheduleStatus.IsClosed();

        await InvokeAsync(StateHasChanged);
    }
}
