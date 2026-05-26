using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;

public partial class FormContractClient
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractClient ContractClient { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }


    private List<State>? States;
    private List<City>? Cities = new();
    private List<Zone>? Zones = new();
    private List<GuidItemModel>? Contractors = new();
    private List<GuidItemModel>? Clients;
    private Client Client = new();
    private Guid Value = Guid.Empty;
    private List<IntItemModel>? WorkStatuses;
    private string ValueText = string.Empty;
    private string BaseView = "/contractclients";
    private string BaseClient = "/api/v1/clients";
    private string BaseComboStatus = "/api/v1/contractclients/loadStatus";
    private string BaseComboContractor = "/api/v1/combosData/ComboContractor";
    private string BaseComboClients = "/api/v1/combosData/ComboClients";
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseComboZone = "/api/v1/zones/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
        await LoadContractor();
        await LoadStatus();
        if (IsEditControl)
        {
            Value = ContractClient.ClientId;
            ValueText = $"{ContractClient.Client!.FirstName} {ContractClient.Client!.LastName}";
        }
    }
    private async Task LoadStatus()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboStatus}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        WorkStatuses = responseHttp.Response;
    }

    private async Task StatusChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            if (int.TryParse(e?.Value?.ToString(), out int modelo))
            {
                if (value == 1) { ContractClient.ContractState = ContractState.Draft; }
                if (value == 2) { ContractClient.ContractState = ContractState.PendingApproval; }
                if (value == 3) { ContractClient.ContractState = ContractState.Active; }
                if (value == 4) { ContractClient.ContractState = ContractState.Exempt; }
                if (value == 5) { ContractClient.ContractState = ContractState.Suspended; }
                if (value == 6) { ContractClient.ContractState = ContractState.Cancelled; }
                if (value == 7) { ContractClient.ContractState = ContractState.Terminated; }
            }
        }
    }

    private async Task OnClientSelected(GuidItemModel item)
    {
        ContractClient.ClientId = item.Value;
        await ClientChanged(item.Value);
    }

    private async Task ClientChanged(Guid clientid)
    {
            ContractClient.ClientId = clientid;
            if (!IsEditControl)
            {
                var responseHttp = await _repository.GetAsync<Client>($"{BaseClient}/{clientid}");
                bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
                if (errorHandler)
                {
                    _navigationManager.NavigateTo($"{BaseView}");
                    return;
                }
                Client = responseHttp.Response!;
                ContractClient.PhoneNumber = Client.PhoneNumber;
                ContractClient.Address = Client.Address;
            }
    }

    private async Task LoadContractor()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseComboContractor}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Contractors = responseHttp.Response;
    }

    private async Task ContractorChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var contractorid))
        {
            ContractClient.ContractorId = contractorid;
        }
    }

    private async Task LoadState()
    {
        var responseHttp = await _repository.GetAsync<List<State>>($"{BaseComboState}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        States = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadCity(ContractClient!.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var stateId))
        {
            ContractClient.StateId = stateId;
            ContractClient.CityId = 0;
            Cities = new();
            Zones = new();
            await LoadCity(stateId);
        }
    }

    private async Task LoadCity(int id)
    {
        var responseHttp = await _repository.GetAsync<List<City>>($"{BaseComboCity}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Cities = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadZone(ContractClient!.CityId);
        }
    }

    private async Task CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var cityId))
        {
            ContractClient.CityId = cityId;
            Zones = new();
            await LoadZone(cityId);
        }
    }

    private async Task LoadZone(int id)
    {
        var responseHttp = await _repository.GetAsync<List<Zone>>($"{BaseComboZone}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Zones = responseHttp.Response;
    }

    private void ZoneChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var zoneId))
        {
            ContractClient.ZoneId = zoneId;
        }
    }
}