using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesNet.NodePage;

public partial class FormNode
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Node Node { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<State>? States;
    private List<City>? Cities = new();
    private List<Zone>? Zones = new();
    private List<Mark>? Marks = new();
    private List<MarkModel>? MarkModels = new();
    private List<IpNetwork>? IpNetworks = new();
    private List<IntItemModel>? Operations = new();
    private List<IntItemModel>? Channels = new();
    private List<IntItemModel>? Securities = new();
    private List<IntItemModel>? FrecuencyTypes = new();
    private List<IntItemModel>? Frecuencies = new();
    private bool showClave = false;
    private string BaseView = "/zones";
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseComboZone = "/api/v1/zones/loadCombo";
    private string BaseComboMark = "/api/v1/marks/loadCombo";
    private string BaseComboMarkModel = "/api/v1/marksmodels/loadCombo";
    private string BaseComboIpNetwork = "/api/v1/ipnetworks/loadCombo";

    private string BaseComboOperation = "/api/v1/downData/OperationCombo";
    private string BaseComboChannels = "/api/v1/downData/channelCombo";
    private string BaseComboSecurity = "/api/v1/downData/SecurityCombo";
    private string BaseComboFrecuencyType = "/api/v1/downData/FreCuentyTypeCombo";
    private string BaseComboFrecuency = "/api/v1/downData/frecuency";

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
        await LoadMarks();
        await LoadIpNetwork();
        await LoadOperation();
        await LoadChannel();
        await LoadSecurity();
        await LoadFrecuencyType();
    }

    private void TogglePasswordVisibility()
    {
        showClave = !showClave;
    }

    private async Task LoadFrecuencyType()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboFrecuencyType}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        FrecuencyTypes = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadFrecuency(Node.FrecuencyTypeId);
        }
    }

    private async Task FrecuencyTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var frecuencyType))
        {
            Node.FrecuencyTypeId = frecuencyType;
            Node.FrecuencyId = 0;
            await LoadFrecuency(frecuencyType);
        }
    }

    private async Task LoadFrecuency(int? id)
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboFrecuency}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Frecuencies = responseHttp.Response;
    }

    private void FrecuencyChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var frecuencyId))
        {
            Node.FrecuencyId = frecuencyId;
        }
    }

    private async Task LoadSecurity()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboSecurity}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Securities = responseHttp.Response;
    }

    private void SecurityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var securityId))
        {
            Node.SecurityId = securityId;
        }
    }

    private async Task LoadChannel()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboChannels}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Channels = responseHttp.Response;
    }

    private void ChannelChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var channelId))
        {
            Node.ChannelId = channelId;
        }
    }

    private async Task LoadOperation()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseComboOperation}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Operations = responseHttp.Response;
    }

    private void OperationChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var operationId))
        {
            Node.OperationId = operationId;
        }
    }

    private async Task LoadIpNetwork()
    {
        if (IsEditControl)
        {
            var responseHttp2 = await _repository.GetAsync<List<IpNetwork>>($"{BaseComboIpNetwork}/{Node.IpNetworkId}");
            bool errorHandler2 = await _responseHandler.HandleErrorAsync(responseHttp2);
            if (errorHandler2)
            {
                _navigationManager.NavigateTo($"{BaseView}");
                return;
            }
            IpNetworks = responseHttp2.Response;
        }
        else
        {

            var responseHttp = await _repository.GetAsync<List<IpNetwork>>($"{BaseComboIpNetwork}");
            bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
            if (errorHandler)
            {
                _navigationManager.NavigateTo($"{BaseView}");
                return;
            }
            IpNetworks = responseHttp.Response;
        }
    }

    private void IpNetworkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var ipnetworkId))
        {
            Node.IpNetworkId = ipnetworkId;
        }
    }

    private async Task LoadMarks()
    {
        var responseHttp = await _repository.GetAsync<List<Mark>>($"{BaseComboMark}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Marks = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadMarkModel(Node!.MarkId);
        }
    }

    private async Task MarkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markId))
        {
            Node.MarkId = markId;
            Node.MarkModelId = Guid.Empty;
            await LoadMarkModel(markId);
        }
    }

    private async Task LoadMarkModel(Guid id)
    {
        var responseHttp = await _repository.GetAsync<List<MarkModel>>($"{BaseComboMarkModel}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        MarkModels = responseHttp.Response;
    }

    private void MarkModelChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markModelId))
        {
            Node.MarkModelId = markModelId;
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
            await LoadCity(Node!.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var stateId))
        {
            Node.StateId = stateId;
            Node.CityId = 0;
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
            await LoadZone(Node!.CityId);
        }
    }

    private async Task CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var cityId))
        {
            Node.CityId = cityId;
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
            Node.ZoneId = zoneId;
        }
    }
}