using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.commissions;

// Comisiones de contratistas: lo que se le causo a cada contratista en el periodo y lo que
// se le queda debiendo.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Lee de las cuentas
// por pagar de los contratistas, que es donde ya vive el dato: aqui no se recalcula nada.
//
// El endpoint devuelve la lista completa del periodo (una fila por contratista, no por
// comision), por eso NO pagina y la pantalla no lleva paginador.
public partial class ReportCommissionsIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-finance";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportCommissionsRow> _rows = new();

    //Arranca con el mes en curso, igual que en la web
    [ObservableProperty]
    private DateTime _dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero y la fila de totales =====
    //Solo se pintan cuando hay datos: sin filas no hay nada que sumar
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private string _sumCommissionsText = "0";

    [ObservableProperty]
    private decimal _sumBaseAmount;

    [ObservableProperty]
    private decimal _sumTotal;

    [ObservableProperty]
    private decimal _sumPaid;

    [ObservableProperty]
    private decimal _sumBalance;

    public string EmptyText => "No hay comisiones en el periodo elegido.";

    public ReportCommissionsIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir ya trae el mes en curso: el reporte no espera a que pulsen Buscar
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
            await LoadAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAsync()
    {
        var periodo = $"datestart={DateStart:yyyy-MM-dd}&dateend={DateEnd:yyyy-MM-dd}";

        var response = await _repository.GetAsync<List<ReportContractorCommissionDto>>(
            $"{BaseUrl}/contractors?{periodo}");

        //Si falla se deja en pantalla lo que ya se estaba viendo
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportContractorCommissionDto>();

        Rows = new ObservableCollection<ReportCommissionsRow>(lista.Select(x => new ReportCommissionsRow(x)));

        CalcularTotales(lista);
    }

    // Los totales se suman aqui una sola vez; el XAML no calcula
    private void CalcularTotales(List<ReportContractorCommissionDto> lista)
    {
        SumCommissionsText = lista.Sum(x => x.Commissions).ToString("N0");
        SumBaseAmount = lista.Sum(x => x.BaseAmount);
        SumTotal = lista.Sum(x => x.Total);
        SumPaid = lista.Sum(x => x.Paid);
        SumBalance = lista.Sum(x => x.Balance);

        HasSummary = lista.Count > 0;
    }
}

// Un contratista con lo suyo del periodo, ya listo para pintar
public class ReportCommissionsRow
{
    public ReportContractorCommissionDto Item { get; }

    public string Name => Item.Name;

    //Es un conteo de comisiones, no dinero: va sin decimales
    public string CommissionsText => Item.Commissions.ToString("N0");

    public decimal BaseAmount => Item.BaseAmount;

    public decimal Total => Item.Total;

    public decimal Paid => Item.Paid;

    public decimal Balance => Item.Balance;

    public ReportCommissionsRow(ReportContractorCommissionDto item)
    {
        Item = item;
    }
}
