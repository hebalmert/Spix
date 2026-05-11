using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesNet.ServerPage;

public partial class FormServer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Server Server { get; set; } = null!;
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
    private bool showClave = false;
    private string BaseView = "/servers";
    private string BaseComboState = "/api/v1/combosData/ComboState";
    private string BaseComboCity = "/api/v1/combosData/ComboCity";
    private string BaseComboZone = "/api/v1/zones/loadCombo";
    private string BaseComboMark = "/api/v1/marks/loadCombo";
    private string BaseComboMarkModel = "/api/v1/marksmodels/loadCombo";
    private string BaseComboIpNetwork = "/api/v1/ipnetworks/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
        await LoadMarks();
        await LoadIpNetwork();
    }

    private void TogglePasswordVisibility()
    {
        showClave = !showClave;
    }

    private async Task LoadIpNetwork()
    {
        if (IsEditControl)
        {
            var responseHttp2 = await _repository.GetAsync<List<IpNetwork>>($"{BaseComboIpNetwork}/{Server.IpNetworkId}");
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
            Server.IpNetworkId = ipnetworkId;
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
            await LoadMarkModel(Server!.MarkId);
        }
    }

    private async Task MarkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markId))
        {
            Server.MarkId = markId;
            Server.MarkModelId = Guid.Empty;
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
            Server.MarkModelId = markModelId;
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
            await LoadCity(Server!.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var stateId))
        {
            Server.StateId = stateId;
            Server.CityId = 0;
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
            await LoadZone(Server!.CityId);
        }
    }

    private async Task CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var cityId))
        {
            Server.CityId = cityId;
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
            Server.ZoneId = zoneId;
        }
    }
}