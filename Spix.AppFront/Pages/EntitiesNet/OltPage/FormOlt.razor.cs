using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesNet.OltPage;

public partial class FormOlt
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Olt Olt { get; set; } = null!;
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
    private string CoordinatesText { get; set; } = string.Empty;

    private string BaseView = "/olts";
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

    protected override void OnParametersSet()
    {
        if (Olt.Latitude.HasValue && Olt.Longitude.HasValue && string.IsNullOrWhiteSpace(CoordinatesText))
        {
            CoordinatesText = $"{Olt.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, {Olt.Longitude.Value.ToString(CultureInfo.InvariantCulture)}";
        }
    }

    private void TogglePasswordVisibility()
    {
        showClave = !showClave;
    }

    private async Task CoordinatesChanged(ChangeEventArgs e)
    {
        CoordinatesText = e.Value?.ToString() ?? string.Empty;
        var parts = CoordinatesText.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !decimal.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var latitude) ||
            !decimal.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var longitude))
        {
            await _sweetAlert.FireAsync("Coordenadas", "Formato invalido. Use: 25.82370270482433, -80.38556718743175", SweetAlertIcon.Warning);
            return;
        }

        Olt.Latitude = latitude;
        Olt.Longitude = longitude;
    }

    // ---------- La IP de red por donde se le entra ----------

    private async Task LoadIpNetwork()
    {
        //Al editar se pide con el id para que la suya venga en la lista aunque este tomada
        var url = IsEditControl ? $"{BaseComboIpNetwork}/{Olt.IpNetworkId}" : BaseComboIpNetwork;

        var responseHttp = await _repository.GetAsync<List<IpNetwork>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        IpNetworks = responseHttp.Response;
    }

    private void IpNetworkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var ipNetworkId))
        {
            Olt.IpNetworkId = ipNetworkId;
        }
    }

    // ---------- Marca y modelo ----------

    private async Task LoadMarks()
    {
        var responseHttp = await _repository.GetAsync<List<Mark>>($"{BaseComboMark}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Marks = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadMarkModel(Olt.MarkId);
        }
    }

    private async Task MarkChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var markId))
        {
            Olt.MarkId = markId;
            Olt.MarkModelId = Guid.Empty;
            await LoadMarkModel(markId);
        }
    }

    private async Task LoadMarkModel(Guid id)
    {
        var responseHttp = await _repository.GetAsync<List<MarkModel>>($"{BaseComboMarkModel}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
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
            Olt.MarkModelId = markModelId;
        }
    }

    // ---------- Donde esta: estado, ciudad y zona ----------

    private async Task LoadState()
    {
        var responseHttp = await _repository.GetAsync<List<State>>($"{BaseComboState}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        States = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadCity(Olt.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var stateId))
        {
            Olt.StateId = stateId;
            Olt.CityId = 0;
            Cities = new();
            Zones = new();
            await LoadCity(stateId);
        }
    }

    private async Task LoadCity(int id)
    {
        var responseHttp = await _repository.GetAsync<List<City>>($"{BaseComboCity}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Cities = responseHttp.Response;
        if (IsEditControl)
        {
            await LoadZone(Olt.CityId);
        }
    }

    private async Task CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var cityId))
        {
            Olt.CityId = cityId;
            Zones = new();
            await LoadZone(cityId);
        }
    }

    private async Task LoadZone(int id)
    {
        var responseHttp = await _repository.GetAsync<List<Zone>>($"{BaseComboZone}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
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
            Olt.ZoneId = zoneId;
        }
    }
}
