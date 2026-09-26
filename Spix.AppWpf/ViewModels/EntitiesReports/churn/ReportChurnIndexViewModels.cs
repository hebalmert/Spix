using CommunityToolkit.Mvvm.ComponentModel;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.churn;

// Contratos fuera de servicio: quien ya no esta pagando y cuanto se dejo de facturar por eso.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. No tiene filtros
// porque el contrato no guarda la fecha en que se retiro: esto es la foto de hoy, no un
// periodo, y por eso tampoco pagina.
public partial class ReportChurnIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-operation";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportChurnZoneRow> _rows = new();

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero =====
    [ObservableProperty]
    private bool _hasSummary;

    //El tablero va en texto ya armado porque cada tarjeta muestra DOS numeros, el conteo y
    //el monto, y la tarjeta compartida solo tiene un renglon para el valor
    [ObservableProperty]
    private string _terminatedText = string.Empty;

    [ObservableProperty]
    private string _cancelledText = string.Empty;

    [ObservableProperty]
    private string _suspendedText = string.Empty;

    [ObservableProperty]
    private string _lostText = string.Empty;

    //===== El pie de la tabla =====
    [ObservableProperty]
    private string _totalsText = string.Empty;

    public string NoteText =>
        "Esta es la foto de hoy: el contrato no guarda la fecha en que se retiro, por eso no se puede contar por periodo.";

    public string EmptyText => "No hay bajas para mostrar";

    public ReportChurnIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir se baja todo de una vez: no hay nada que elegir antes
    public async Task InitializeAsync()
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

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportChurnDto>($"{BaseUrl}/churn/summary");

        //Si falla se deja el tablero como estaba
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportChurnDto();

        TerminatedText = Tarjeta(datos.Terminated, datos.TerminatedAmount);
        CancelledText = Tarjeta(datos.Cancelled, datos.CancelledAmount);
        SuspendedText = Tarjeta(datos.Suspended, datos.SuspendedAmount);

        //Lo que se dejo de facturar por los que ya no estan: los suspendidos no cuentan,
        //todavia pueden volver
        LostText = (datos.TerminatedAmount + datos.CancelledAmount).ToString("N2");

        HasSummary = true;
    }

    private static string Tarjeta(int contratos, decimal monto) =>
        $"{contratos:N0}  -  {monto:N2}";

    private async Task LoadZonesAsync()
    {
        var response = await _repository.GetAsync<List<ReportChurnZoneDto>>($"{BaseUrl}/churn/zones");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportChurnZoneDto>();

        Rows = new ObservableCollection<ReportChurnZoneRow>(lista.Select(x => new ReportChurnZoneRow(x)));

        //El total de la tabla, que en la web es la ultima fila del cuadro
        TotalsText = $"Total: {lista.Sum(x => x.Contracts):N0} contratos  -  {lista.Sum(x => x.Amount):N2}";
    }
}

// Las bajas de una zona, ya listas para pintar
public class ReportChurnZoneRow
{
    public ReportChurnZoneDto Item { get; }

    public string Name => Item.Name;

    public int Contracts => Item.Contracts;

    public decimal Amount => Item.Amount;

    public ReportChurnZoneRow(ReportChurnZoneDto item)
    {
        Item = item;
    }
}
