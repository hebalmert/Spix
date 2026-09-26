using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.cutoff;

// Efectividad del corte: como le fue al corte del periodo.
//
// Arriba el tablero (cuantos se cortaron y cuantos pagaron por eso) y abajo una fila por
// zona, para ver donde se corto mas y donde respondieron mejor.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca la Mikrotik. Los dos endpoints
// no paginan, por eso no hay paginacion y el pie del panel lo ocupan los totales.
public partial class ReportCutOffIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-operation";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportCutOffZoneRow> _rows = new();

    //Arranca con el mes en curso, igual que la pantalla web
    [ObservableProperty]
    private DateTime _dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _cut;

    [ObservableProperty]
    private int _recovered;

    [ObservableProperty]
    private int _reactivated;

    [ObservableProperty]
    private int _stillDown;

    //De cada cien cortados, cuantos pagaron. Llega armado como texto porque el porcentaje
    //no es un dato del backend sino una division que solo tiene sentido ya redondeada.
    [ObservableProperty]
    private string _rateText = "0 %";

    //El indicador compartido solo tiene rotulo y numero, y no se toca por una pantalla:
    //por eso el monto de la web viaja dentro del rotulo en vez de una tercera linea.
    [ObservableProperty]
    private string _cutLabel = "Cortados";

    [ObservableProperty]
    private string _recoveredLabel = "Recuperados";

    [ObservableProperty]
    private string _stillDownLabel = "Siguen abajo";

    //===== Los totales de la tabla de zonas =====
    [ObservableProperty]
    private int _totalCut;

    [ObservableProperty]
    private int _totalRecovered;

    [ObservableProperty]
    private decimal _totalAmount;

    public ReportCutOffIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // La web ya muestra el mes en curso al entrar: aqui se busca igual sin esperar al boton
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
            await LoadZonesAsync();
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
        var response = await _repository.GetAsync<ReportCutOffSummaryDto>($"{BaseUrl}/cutoff/summary?{Periodo()}");

        //Si falla se deja en pantalla lo del periodo anterior
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportCutOffSummaryDto();

        Cut = datos.Cut;
        Recovered = datos.Recovered;
        Reactivated = datos.Reactivated;
        StillDown = datos.StillDown;

        CutLabel = $"Cortados - {datos.CutAmount:N2}";
        RecoveredLabel = $"Recuperados - {datos.RecoveredAmount:N2}";
        StillDownLabel = $"Siguen abajo - {datos.StillDownAmount:N2}";

        //Sin cortes no hay de que sacar porcentaje: seria dividir por cero
        var tasa = datos.Cut == 0
            ? 0
            : Math.Round(datos.Recovered * 100m / datos.Cut, 0);

        RateText = $"{tasa:N0} %";

        HasSummary = true;
    }

    private async Task LoadZonesAsync()
    {
        var response = await _repository.GetAsync<List<ReportCutOffZoneDto>>($"{BaseUrl}/cutoff/zones?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportCutOffZoneDto>();

        Rows = new ObservableCollection<ReportCutOffZoneRow>(lista.Select(x => new ReportCutOffZoneRow(x)));

        TotalCut = lista.Sum(x => x.Cut);
        TotalRecovered = lista.Sum(x => x.Recovered);
        TotalAmount = lista.Sum(x => x.Amount);
    }
}

// Una zona del corte, ya lista para pintar
public class ReportCutOffZoneRow
{
    public ReportCutOffZoneDto Item { get; }

    public string Name => Item.Name;

    public int Cut => Item.Cut;

    public int Recovered => Item.Recovered;

    public decimal Amount => Item.Amount;

    public ReportCutOffZoneRow(ReportCutOffZoneDto item)
    {
        Item = item;
    }
}
