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
    private List<GuidItemModel>? EstratosSociales;
    private string ValueText = string.Empty;
    private string BaseView = "/contractclients";
    private string BaseClient = "/api/v1/clients";
    //En este modulo el contrato solo se mueve entre Draft, Por aprobacion y En progreso.
    //Activar, suspender, anular o retirar se hace en DetailContractControl, que es donde
    //se toca el MikroTik: si se cambiara aqui, el contrato quedaria suspendido pero con servicio.
    private string BaseComboStatus = "/api/v1/contractclients/loadContractClientStatus";
    private string BaseComboContractor = "/api/v1/combosData/ComboContractor";
    private string BaseComboClients = "/api/v1/combosData/ComboClients";
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseComboZone = "/api/v1/zones/loadCombo";
    private string BaseComboEstratoSocial = "/api/v1/estratossociales/loadCombo";

    //Se enciende cuando intentan guardar sin elegir estado
    private bool StatusMissing;

    //Valor del combo de estado: un int, igual que CountryId en FormCorporation
    private int StatusValue => (int)ContractClient.ContractState;

    //El estado solo se edita aqui mientras el contrato se esta armando. Ya operativo
    //(Activo, Exento, Suspendido, Anulado, Terminado) se muestra pero no se cambia.
    private bool StatusEditable =>
        !IsEditControl ||
        ContractClient.ContractState == ContractState.Draft ||
        ContractClient.ContractState == ContractState.PendingApproval ||
        ContractClient.ContractState == ContractState.InProgress;

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
        await LoadContractor();
        await LoadStatus();
        await LoadEstratosSociales();
        if (IsEditControl)
        {
            Value = ContractClient.ClientId;
            ValueText = $"{ContractClient.Client!.FirstName} {ContractClient.Client!.LastName}";
        }
    }
    //Correo del cliente: en creacion viene del cliente que se acaba de elegir,
    //en edicion del cliente ya guardado en el contrato
    private string? ClientEmail => IsEditControl
        ? ContractClient?.Client?.Email
        : Client?.Email;

    private async Task LoadStatus()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>(BaseComboStatus);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        WorkStatuses = responseHttp.Response;
    }

    private async Task LoadEstratosSociales()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>(BaseComboEstratoSocial);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo(BaseView);
            return;
        }

        EstratosSociales = responseHttp.Response;
    }

    private void EstratoSocialChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out Guid estratoSocialId) && estratoSocialId != Guid.Empty)
        {
            ContractClient.EstratoSocialId = estratoSocialId;
            return;
        }

        ContractClient.EstratoSocialId = null;
    }

    //Sin estado elegido no se manda nada al servidor
    private async Task HandleSubmitAsync()
    {
        if (ContractClient.ContractState == 0)
        {
            StatusMissing = true;
            return;
        }

        StatusMissing = false;
        await OnSubmit.InvokeAsync();
    }

    private void StatusChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            ContractClient.ContractState = (ContractState)value;
            StatusMissing = false;
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
