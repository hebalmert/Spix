using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.byzone;

// Contratos por zona: los contratos de una zona con su plan y lo que factura esa zona.
//
// A la zona se llega bajando por estado y ciudad, porque asi se descarta de una vez todo
// lo que no existe: cada lista la arma el backend solo con lo que la corporacion si tiene.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik.
public partial class ReportByZoneIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportByZoneRow> _rows = new();

    //Las cuatro listas las arma el backend con su neutro en la posicion 0: aqui no se tocan
    [ObservableProperty]
    private ObservableCollection<IntItemModel> _states = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _cities = new();

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _zones = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _contractStates = new();

    [ObservableProperty]
    private int _stateId;

    [ObservableProperty]
    private int _cityId;

    [ObservableProperty]
    private Guid _zoneId;

    //Arranca mostrando los activos
    [ObservableProperty]
    private int _contractStateId = (int)ContractState.Active;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== Lo que factura la zona elegida =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _contracts;

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private int _withoutPlan;

    public string EmptyText => "Elija estado, ciudad y zona para ver sus contratos.";

    public ReportByZoneIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir solo bajan las dos listas que no dependen de nada
    public async Task InitializeAsync()
    {
        var estados = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocontractstates");
        if (!await _responseHandler.HandleErrorAsync(estados))
        {
            ContractStates = new ObservableCollection<IntItemModel>(estados.Response ?? new List<IntItemModel>());
        }

        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combostates");
        if (!await _responseHandler.HandleErrorAsync(response))
        {
            States = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
        }
    }

    // La pantalla no tiene boton de Buscar: cada filtro que cambia vuelve a preguntar,
    // igual que en Blazor. Por eso el cambio de la propiedad dispara la peticion.
    partial void OnStateIdChanged(int value)
    {
        _ = CambioEstadoAsync();
    }

    partial void OnCityIdChanged(int value)
    {
        _ = CambioCiudadAsync();
    }

    partial void OnZoneIdChanged(Guid value)
    {
        _ = CambioZonaAsync();
    }

    partial void OnContractStateIdChanged(int value)
    {
        _ = CambioEstadoContratoAsync();
    }

    private async Task CambioEstadoAsync()
    {
        CityId = 0;
        Cities = new ObservableCollection<IntItemModel>();
        LimpiarZona();

        if (StateId == 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocities/{StateId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Cities = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    private async Task CambioCiudadAsync()
    {
        LimpiarZona();

        if (CityId == 0)
        {
            return;
        }

        var response = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combozones/{CityId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Zones = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    private async Task CambioZonaAsync()
    {
        if (ZoneId == Guid.Empty)
        {
            LimpiarResultado();
            return;
        }

        await BuscarAsync();
    }

    // Cambiar el estado vuelve a preguntar, con la misma zona elegida
    private async Task CambioEstadoContratoAsync()
    {
        if (ZoneId == Guid.Empty)
        {
            return;
        }

        await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        IsLoading = true;

        try
        {
            CurrentPage = 1;

            await LoadSummaryAsync();
            await LoadAsync(1);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Al cambiar de estado o de ciudad la zona anterior ya no vale, y lo que se veia tampoco
    private void LimpiarZona()
    {
        ZoneId = Guid.Empty;
        Zones = new ObservableCollection<GuidItemModel>();
        LimpiarResultado();
    }

    private void LimpiarResultado()
    {
        HasSummary = false;
        Rows = new ObservableCollection<ReportByZoneRow>();
        TotalPages = 0;
        CurrentPage = 1;
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportActiveSummaryDto>(
            $"{BaseUrl}/by-zone/{ZoneId}/summary?stateid={ContractStateId}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportActiveSummaryDto();

        Contracts = datos.Contracts;
        MonthlyTotal = datos.MonthlyTotal;
        WithoutPlan = datos.WithoutPlan;

        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        var response = await _repository.GetAsync<List<ReportActiveContractDto>>(
            $"{BaseUrl}/by-zone/{ZoneId}?page={page}&recordsnumber={PageSize}&stateid={ContractStateId}");

        //Si falla se deja lo que hubiera en pantalla
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<ReportByZoneRow>(
            (response.Response ?? new List<ReportActiveContractDto>()).Select(x => new ReportByZoneRow(x)));

        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    // Paginar NO vuelve a pedir el tablero: esos numeros son los de toda la zona
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }
}

// Un contrato de la zona, ya listo para pintar
public class ReportByZoneRow
{
    public ReportActiveContractDto Item { get; }

    public string? ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    // La pastilla solo sale cuando el contrato no esta activo, por eso la vista la esconde
    public bool Active => Item.IsActive;

    public string? ServerName => Item.ServerName;

    public string? PlanName => Item.PlanName;

    public decimal PlanPrice => Item.PlanPrice;

    public ReportByZoneRow(ReportActiveContractDto item)
    {
        Item = item;
    }
}
