using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Los contratos de una zona con su plan y lo que factura esa zona.
//Se llega a la zona por estado y ciudad: cada lista la arma el backend con lo que existe.
public partial class ReportByZone
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private List<IntItemModel>? States;
    private List<IntItemModel>? ContractStates;
    private List<IntItemModel>? Cities = new();
    private List<GuidItemModel>? Zones = new();
    private List<ReportActiveContractDto>? Contracts = new();
    private ReportActiveSummaryDto? Summary;

    private int StateId;

    //Arranca mostrando los activos
    private int ContractStateId = (int)ContractState.Active;
    private int CityId;
    private Guid ZoneId;
    private int CurrentPage = 1;
    private int TotalPages;

    protected override async Task OnInitializedAsync()
    {
        var estados = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocontractstates");
        if (!await _responseHandler.HandleErrorAsync(estados))
            ContractStates = estados.Response ?? new();

        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combostates");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            States = responseHttp.Response ?? new();
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        StateId = int.TryParse(e.Value?.ToString(), out var id) ? id : 0;
        CityId = 0;
        Cities = new();
        LimpiarZona();

        if (StateId == 0)
            return;

        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocities/{StateId}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Cities = responseHttp.Response ?? new();
    }

    private async Task CityChanged(ChangeEventArgs e)
    {
        CityId = int.TryParse(e.Value?.ToString(), out var id) ? id : 0;
        LimpiarZona();

        if (CityId == 0)
            return;

        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combozones/{CityId}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Zones = responseHttp.Response ?? new();
    }

    private async Task ZoneChanged(ChangeEventArgs e)
    {
        ZoneId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;

        if (ZoneId == Guid.Empty)
        {
            Summary = null;
            Contracts = new();
            TotalPages = 0;
            return;
        }

        CurrentPage = 1;
        await LoadSummaryAsync();
        await LoadContractsAsync();
    }

    //Cambiar el estado vuelve a preguntar, con la misma zona elegida
    private async Task ContractStateChanged(ChangeEventArgs e)
    {
        ContractStateId = int.TryParse(e.Value?.ToString(), out var id) ? id : 0;

        if (ZoneId == Guid.Empty)
            return;

        CurrentPage = 1;
        await LoadSummaryAsync();
        await LoadContractsAsync();
    }

    private void LimpiarZona()
    {
        ZoneId = Guid.Empty;
        Zones = new();
        Summary = null;
        Contracts = new();
        TotalPages = 0;
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<ReportActiveSummaryDto>($"{BaseUrl}/by-zone/{ZoneId}/summary?stateid={ContractStateId}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task LoadContractsAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<ReportActiveContractDto>>(
            $"{BaseUrl}/by-zone/{ZoneId}?page={page}&recordsnumber={PageSize}&stateid={ContractStateId}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Contracts = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadContractsAsync(page);
    }
}
