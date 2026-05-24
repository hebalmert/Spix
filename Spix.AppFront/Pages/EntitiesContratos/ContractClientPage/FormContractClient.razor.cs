using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
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
    private List<GuidItemModel>? Contractors;
    private List<GuidItemModel>? Clients;
    private Client Client = new();
    private string BaseView = "/contractclients";
    private string BaseClient = "/api/v1/clients";
    private string BaseComboContractor = "/api/v1/combosData/ComboContractor";
    private string BaseComboClients = "/api/v1/combosData/ComboClients";
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseComboZone = "/api/v1/zones/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
        await LoadContractor();
        await LoadClients();
    }

    private async Task LoadClients()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseComboClients}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Clients = responseHttp.Response;
    }

    private async Task ClientChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var clientid))
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