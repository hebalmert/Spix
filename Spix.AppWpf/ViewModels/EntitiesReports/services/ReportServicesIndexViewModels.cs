using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.services;

// Servicios del periodo: lo que paso con las solicitudes de servicio.
//
// Arriba el tablero (como entraron y como terminaron), y abajo dos listas: que fue lo que
// mas se hizo y quien lo resolvio.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca la Mikrotik. Los tres endpoints
// devuelven el periodo completo y no paginan, por eso no hay paginacion y el pie de cada
// panel lo ocupan los totales.
public partial class ReportServicesIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-operation";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportServiceLineRow> _serviceRows = new();

    [ObservableProperty]
    private ObservableCollection<ReportTechnicianLineRow> _technicianRows = new();

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
    private int _requested;

    [ObservableProperty]
    private int _phoneResolved;

    [ObservableProperty]
    private int _scheduled;

    [ObservableProperty]
    private int _completed;

    [ObservableProperty]
    private int _cancelled;

    [ObservableProperty]
    private decimal _billed;

    //===== Los totales de las dos listas =====
    [ObservableProperty]
    private int _totalTimes;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private int _totalSolved;

    public ReportServicesIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
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
            await LoadServicesAsync();
            await LoadTechniciansAsync();
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
        var response = await _repository.GetAsync<ReportServicesSummaryDto>($"{BaseUrl}/services/summary?{Periodo()}");

        //Si falla se deja en pantalla lo del periodo anterior
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportServicesSummaryDto();

        Requested = datos.Requested;
        PhoneResolved = datos.PhoneResolved;
        Scheduled = datos.Scheduled;
        Completed = datos.Completed;
        Cancelled = datos.Cancelled;
        Billed = datos.Billed;

        HasSummary = true;
    }

    private async Task LoadServicesAsync()
    {
        var response = await _repository.GetAsync<List<ReportServiceDto>>($"{BaseUrl}/services/top?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportServiceDto>();

        ServiceRows = new ObservableCollection<ReportServiceLineRow>(lista.Select(x => new ReportServiceLineRow(x)));

        TotalTimes = lista.Sum(x => x.Times);
        TotalAmount = lista.Sum(x => x.Total);
    }

    private async Task LoadTechniciansAsync()
    {
        var response = await _repository.GetAsync<List<ReportTechnicianDto>>($"{BaseUrl}/services/technicians?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportTechnicianDto>();

        TechnicianRows = new ObservableCollection<ReportTechnicianLineRow>(lista.Select(x => new ReportTechnicianLineRow(x)));

        TotalSolved = lista.Sum(x => x.Services);
    }
}

// Un tipo de servicio del periodo, ya listo para pintar
public class ReportServiceLineRow
{
    public ReportServiceDto Item { get; }

    public string Name => Item.Name;

    public string? CategoryName => Item.CategoryName;

    public int Times => Item.Times;

    public decimal Total => Item.Total;

    public ReportServiceLineRow(ReportServiceDto item)
    {
        Item = item;
    }
}

// Un tecnico y lo que resolvio. El monto del DTO no se pinta: la web solo muestra el
// conteo, y aqui se replica igual para que los dos reportes digan lo mismo.
public class ReportTechnicianLineRow
{
    public ReportTechnicianDto Item { get; }

    public string Name => Item.Name;

    public int Services => Item.Services;

    public ReportTechnicianLineRow(ReportTechnicianDto item)
    {
        Item = item;
    }
}
