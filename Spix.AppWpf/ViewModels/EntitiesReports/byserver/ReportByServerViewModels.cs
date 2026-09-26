using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.byserver;

// Contratos por servidor: que cuelga de cada equipo y cuanto genera.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Solo se elige un
// servidor y un estado, y se mira lo que sale.
//
// El tablero y la tabla se piden por separado porque el tablero es del servidor completo y
// la tabla llega paginada: paginar NO recalcula los numeros de arriba.
public partial class ReportByServerIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportByServerRow> _rows = new();

    //Las dos listas llegan armadas del backend, con su neutro en la posicion 0: aqui no se
    //filtran, no se ordenan y no se les agrega nada
    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _servers = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _states = new();

    [ObservableProperty]
    private Guid _serverId;

    [ObservableProperty]
    private int _stateId;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== Lo que genera el servidor elegido =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _contracts;

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private int _withoutPlan;

    public string EmptyText => "Elija un servidor para ver sus contratos.";

    public ReportByServerIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir solo bajan las dos listas: nada se consulta hasta elegir un servidor
    public async Task InitializeAsync()
    {
        var estados = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocontractstates");
        if (!await _responseHandler.HandleErrorAsync(estados))
        {
            States = new ObservableCollection<IntItemModel>(estados.Response ?? new List<IntItemModel>());
        }

        //El estado se fija DESPUES de tener la lista: el ComboBox descarta un SelectedValue
        //que todavia no existe entre sus opciones y se quedaria en el neutro
        StateId = (int)ContractState.Active;

        var servidores = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/comboservers");
        if (!await _responseHandler.HandleErrorAsync(servidores))
        {
            Servers = new ObservableCollection<GuidItemModel>(servidores.Response ?? new List<GuidItemModel>());
        }
    }

    // Cambiar de servidor vuelve a preguntar todo; el neutro deja la pantalla en blanco
    partial void OnServerIdChanged(Guid value) => _ = ServerChangedAsync();

    // Cambiar el estado vuelve a preguntar, con el mismo servidor elegido
    partial void OnStateIdChanged(int value) => _ = StateChangedAsync();

    private async Task ServerChangedAsync()
    {
        if (ServerId == Guid.Empty)
        {
            HasSummary = false;
            Rows = new ObservableCollection<ReportByServerRow>();
            TotalPages = 0;
            return;
        }

        await BuscarAsync();
    }

    private async Task StateChangedAsync()
    {
        if (ServerId == Guid.Empty)
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

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportActiveSummaryDto>(
            $"{BaseUrl}/by-server/{ServerId}/summary?stateid={StateId}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
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
            $"{BaseUrl}/by-server/{ServerId}?page={page}&recordsnumber={PageSize}&stateid={StateId}");

        //Si falla se deja lo que hubiera en pantalla
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<ReportByServerRow>(
            (response.Response ?? new List<ReportActiveContractDto>()).Select(x => new ReportByServerRow(x)));

        CurrentPage = page;

        //El total de paginas viaja en el encabezado
        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    // Paginar NO vuelve a pedir el tablero: esos numeros son del servidor completo
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }
}

// Un contrato del servidor, ya listo para pintar
public class ReportByServerRow
{
    public ReportActiveContractDto Item { get; }

    public string ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public string? ZoneName => Item.ZoneName;

    public string? PlanName => Item.PlanName;

    public decimal PlanPrice => Item.PlanPrice;

    //La pastilla solo aparece cuando se estan mirando estados distintos del activo
    public bool IsInactive => !Item.IsActive;

    public ReportByServerRow(ReportActiveContractDto item)
    {
        Item = item;
    }
}
