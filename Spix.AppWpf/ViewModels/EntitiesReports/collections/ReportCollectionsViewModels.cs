using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.collections;

// Recaudo del periodo: lo que entro entre dos fechas, como entro y quien lo recogio.
//
// Los tres bloques (lo que entro, quien lo recogio y las notas emitidas) comparten el MISMO
// rango de fechas, porque asi es como se cierra el dia: son tres miradas del mismo periodo.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Por eso las filas
// no llevan botones y la pantalla no tiene mas accion que Buscar.
public partial class ReportCollectionsIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-finance";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    //===== El periodo =====
    [ObservableProperty]
    private DateTime _dateStart = DateTime.Today;

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    //===== Lo que entro y como entro =====
    //Cada bloque tiene su propia bandera: el backend responde por separado y uno puede
    //fallar sin arrastrar a los otros dos.
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _collections;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _cash;

    [ObservableProperty]
    private decimal _bank;

    [ObservableProperty]
    private decimal _prePayment;

    [ObservableProperty]
    private decimal _discount;

    //===== Quien lo recogio =====
    [ObservableProperty]
    private ObservableCollection<ReportCollectionsCollectorRow> _rows = new();

    [ObservableProperty]
    private int _collectorsCount;

    [ObservableProperty]
    private decimal _collectorsTotal;

    //===== Las notas emitidas en el mismo periodo =====
    [ObservableProperty]
    private bool _hasNotes;

    [ObservableProperty]
    private int _notes;

    [ObservableProperty]
    private decimal _issued;

    [ObservableProperty]
    private decimal _collected;

    [ObservableProperty]
    private decimal _pending;

    public string EmptyText => "Nadie recogio nada en este periodo.";

    public ReportCollectionsIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Arranca mostrando el dia de hoy, que es lo que mas se consulta
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
            await LoadCollectorsAsync();
            await LoadNotesAsync();
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
        var response = await _repository.GetAsync<ReportCollectionSummaryDto>(
            $"{BaseUrl}/collections/summary?{Periodo()}");

        //Si falla se deja en pantalla lo de la busqueda anterior
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportCollectionSummaryDto();

        Collections = datos.Collections;
        Total = datos.Total;
        Cash = datos.Cash;

        //Tarjeta y transferencia van juntas: en el cuadre nunca se miran por separado
        Bank = datos.Card + datos.Transfer;

        PrePayment = datos.PrePayment;
        Discount = datos.Discount;

        HasSummary = true;
    }

    private async Task LoadCollectorsAsync()
    {
        var response = await _repository.GetAsync<List<ReportCollectorDto>>(
            $"{BaseUrl}/collections/collectors?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var lista = response.Response ?? new List<ReportCollectorDto>();

        Rows = new ObservableCollection<ReportCollectionsCollectorRow>(
            lista.Select(x => new ReportCollectionsCollectorRow(x)));

        //El pie de la tabla: la suma de la propia lista, no otra consulta
        CollectorsCount = lista.Sum(x => x.Collections);
        CollectorsTotal = lista.Sum(x => x.Total);
    }

    private async Task LoadNotesAsync()
    {
        var response = await _repository.GetAsync<ReportNotesSummaryDto>(
            $"{BaseUrl}/notes/summary?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new ReportNotesSummaryDto();

        Notes = datos.Notes;
        Issued = datos.Issued;
        Collected = datos.Collected;
        Pending = datos.Pending;

        HasNotes = true;
    }
}

// Lo que recogio una persona en el periodo, ya listo para pintar
public class ReportCollectionsCollectorRow
{
    public ReportCollectorDto Item { get; }

    public string Name => Item.Name;

    public int Collections => Item.Collections;

    public decimal Total => Item.Total;

    public ReportCollectionsCollectorRow(ReportCollectorDto item)
    {
        Item = item;
    }
}
