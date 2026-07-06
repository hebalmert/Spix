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

    [Parameter, EditorRequired] public ServiceRequestDto Model { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private const string BaseUrl = "/api/v1/servicerequests";
    private const string BaseView = "/servicerequests";
    private const string BaseComboStatus = "/api/v1/schedulecontrol/loadStatus";
    private List<ServiceRequestContractDto> Contracts = new();
    private List<GuidItemModel>? Technicians = new();
    private List<IntItemModel>? ScheduleStatuses;
    private List<ServiceCategory> ServiceCategories = new();
    private List<ServiceClient> ServiceClients = new();
    private ServiceRequestDetailDto Detail = new();
    private string ContractFilter = string.Empty;
    private bool IsCompleted;

    protected override async Task OnInitializedAsync()
    {
        await LoadTechnicians();
        await LoadStatus();
        await LoadServiceCategories();

        if (IsEditControl)
        {
            IsCompleted = Model.ScheduleStatus == ScheduleStatus.Completed;
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

    private async Task AddDetail()
    {
        Detail.ServiceRequestId = Model.ServiceRequestId;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/details", Detail);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await ReloadRequest();
        Detail = new() { ServiceRequestId = Model.ServiceRequestId };
        ServiceClients.Clear();
    }

    private async Task DeleteDetail(Guid id)
    {
        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/details/{id}");
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
}
