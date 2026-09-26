using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.contracts;

// Contratos del periodo: los que entraron entre dos fechas y lo que le agregaron a la
// facturacion mensual.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca la Mikrotik. Por eso las filas
// no llevan botones.
//
// El backend no pagina estos dos endpoints: devuelve el resumen y la lista completa de
// planes del periodo, asi que aqui no hay paginacion.
public partial class ReportContractsIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-operation";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportContractsPlanRow> _rows = new();

    //Arranca con el mes en curso, igual que la pantalla web
    [ObservableProperty]
    private DateTime _dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero del periodo =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _newContracts;

    [ObservableProperty]
    private int _installed;

    [ObservableProperty]
    private decimal _monthlyTotal;

    //Los que entraron pero todavia no quedaron dando servicio
    [ObservableProperty]
    private int _pending;

    //===== La fila de totales del cuadro de planes =====
    [ObservableProperty]
    private bool _hasPlans;

    [ObservableProperty]
    private int _totalTimes;

    [ObservableProperty]
    private decimal _totalMonthly;

    public string EmptyText => "No entraron contratos nuevos en el periodo elegido.";

    public ReportContractsIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // La pantalla web ya trae el mes en curso cargado al abrir: aqui se hace igual
    public async Task InitializeAsync()
    {
        await SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        IsLoading = true;

        try
        {
            await LoadSummaryAsync();
            await LoadPlansAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string Periodo() =>
        $"datestart={DateStart:yyyy-MM-dd}&dateend={DateEnd:yyyy-MM-dd}";

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportContractsSummaryDto>(
            $"{BaseUrl}/contracts/summary?{Periodo()}");

        //Si falla se deja en pantalla lo de la busqueda anterior
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportContractsSummaryDto();

        NewContracts = datos.NewContracts;
        Installed = datos.Installed;
        MonthlyTotal = datos.MonthlyTotal;
        Pending = datos.NewContracts - datos.Installed;

        HasSummary = true;
    }

    private async Task LoadPlansAsync()
    {
        var response = await _repository.GetAsync<List<ReportPlanDto>>(
            $"{BaseUrl}/contracts/top-plans?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var planes = response.Response ?? new List<ReportPlanDto>();

        Rows = new ObservableCollection<ReportContractsPlanRow>(planes.Select(x => new ReportContractsPlanRow(x)));

        TotalTimes = planes.Sum(x => x.Times);
        TotalMonthly = planes.Sum(x => x.MonthlyTotal);

        //El pie de totales solo tiene sentido cuando hay planes que sumar
        HasPlans = planes.Count > 0;
    }
}

// Un plan del periodo, ya listo para pintar
public class ReportContractsPlanRow
{
    public ReportPlanDto Item { get; }

    public string Name => Item.Name;

    public int Times => Item.Times;

    public decimal MonthlyTotal => Item.MonthlyTotal;

    public ReportContractsPlanRow(ReportPlanDto item)
    {
        Item = item;
    }
}
