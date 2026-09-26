using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.aging;

// Cartera por antiguedad: la plata que falta por cobrar, repartida por lo vieja que es la nota.
//
// El tablero es la escalera de la cartera: mientras mas a la derecha, mas dificil de cobrar.
// Debajo va la lista corta de los que mas deben, que es con la que se sale a cobrar.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Por eso las filas
// no llevan botones y la pantalla solo sabe volver a preguntar.
public partial class ReportAgingIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-finance";

    //Solo se muestran los 20 que mas deben: la lista es para salir a cobrar, no para leerla entera
    private const int TopDebtors = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportAgingDebtorRow> _rows = new();

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero =====
    //Solo aparece cuando el resumen llego bien: un tablero en ceros se lee como "no hay cartera"
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private string _balanceLabel = "Cartera total";

    [ObservableProperty]
    private decimal _balance;

    [ObservableProperty]
    private string _currentLabel = "Hasta 30 dias";

    [ObservableProperty]
    private decimal _current;

    [ObservableProperty]
    private string _days30Label = "De 31 a 60 dias";

    [ObservableProperty]
    private decimal _days30;

    [ObservableProperty]
    private string _days60Label = "De 61 a 90 dias";

    [ObservableProperty]
    private decimal _days60;

    [ObservableProperty]
    private string _days90Label = "Mas de 90 dias";

    [ObservableProperty]
    private decimal _days90;

    public ReportAgingIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir se baja todo: este reporte no tiene filtros, es la foto de hoy
    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            await LoadSummaryAsync();
            await LoadDebtorsAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportAgingDto>($"{BaseUrl}/aging/summary");

        //Si falla se deja en pantalla el tablero anterior
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportAgingDto();

        //El conteo de notas viaja pegado al rotulo: el indicador solo tiene un numero
        BalanceLabel = Rotulo("Cartera total", datos.Notes);
        Balance = datos.Balance;

        CurrentLabel = Rotulo("Hasta 30 dias", datos.NotesCurrent);
        Current = datos.Current;

        Days30Label = Rotulo("De 31 a 60 dias", datos.Notes30);
        Days30 = datos.Days30;

        Days60Label = Rotulo("De 61 a 90 dias", datos.Notes60);
        Days60 = datos.Days60;

        Days90Label = Rotulo("Mas de 90 dias", datos.Notes90);
        Days90 = datos.Days90;

        HasSummary = true;
    }

    private static string Rotulo(string titulo, int notas) =>
        $"{titulo} ({notas:N0} notas)";

    private async Task LoadDebtorsAsync()
    {
        var response = await _repository.GetAsync<List<ReportDebtorDto>>($"{BaseUrl}/aging/debtors?top={TopDebtors}");

        //Si falla se deja lo que hubiera en pantalla
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<ReportAgingDebtorRow>(
            (response.Response ?? new List<ReportDebtorDto>()).Select(x => new ReportAgingDebtorRow(x)));
    }
}

// Un contrato que debe, ya listo para pintar
public class ReportAgingDebtorRow
{
    public ReportDebtorDto Item { get; }

    public string? ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public string? ZoneName => Item.ZoneName;

    public int Notes => Item.Notes;

    public int Days => Item.Days;

    public decimal Balance => Item.Balance;

    public ReportAgingDebtorRow(ReportDebtorDto item)
    {
        Item = item;
    }
}
