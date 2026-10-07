using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.stock;

// Movimientos de inventario: que entro y que salio de cada bodega, y cuanto hay hoy.
// Replica del reporte /reports/stock de la web.
//
// Es de SOLA LECTURA: no crea, no edita y no toca el MikroTik. Por eso las filas no llevan
// botones y no hay formulario detras.
//
// El backend DEDUCE los movimientos de las compras y los traslados cerrados: el sistema no
// guarda una tabla de movimientos, el Stock es un acumulado. Aqui solo se pintan.
public partial class ReportStockIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-inventory";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportStockMoveDto> _moves = new();

    [ObservableProperty]
    private ObservableCollection<ReportStockBalanceDto> _balance = new();

    //Las bodegas llegan ARMADAS del backend, con su neutro
    [ObservableProperty]
    private ObservableCollection<ProductStorage> _storages = new();

    [ObservableProperty]
    private Guid _storageId;

    [ObservableProperty]
    private DateTime? _desde = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime? _hasta = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero =====
    [ObservableProperty]
    private bool _hasSummary;

    //Los numeros viajan ya formateados porque el indicador solo sabe pintar texto
    [ObservableProperty]
    private string _summaryEntriesText = string.Empty;

    [ObservableProperty]
    private string _summaryExitsText = string.Empty;

    [ObservableProperty]
    private string _summaryMovesText = string.Empty;

    [ObservableProperty]
    private string _summaryOnHandText = string.Empty;

    public ReportStockIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    public async Task InitializeAsync()
    {
        await LoadStoragesAsync();
        await RefreshAsync();
    }

    //El combo de bodegas: una sola vez al abrir
    private async Task LoadStoragesAsync()
    {
        var response = await _repository.GetAsync<List<ProductStorage>>("api/v1/combosData/ComboStorage");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Storages = new ObservableCollection<ProductStorage>(response.Response ?? new List<ProductStorage>());
    }

    // El tablero, los movimientos y las existencias. Si el tablero falla la pantalla NO se
    // cae: simplemente no se pinta.
    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsLoading = true;

        try
        {
            await LoadSummaryAsync();
            await LoadMovesAsync();
            await LoadBalanceAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    //El filtro que comparten el tablero y los movimientos
    private string Filtro()
    {
        var filtro = $"?desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}";

        if (StorageId != Guid.Empty)
        {
            filtro += $"&storageId={StorageId}";
        }

        return filtro;
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportStockSummaryDto>($"{BaseUrl}/stock/summary{Filtro()}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new ReportStockSummaryDto();

        SummaryEntriesText = datos.Entries.ToString("N2");
        SummaryExitsText = datos.Exits.ToString("N2");
        SummaryMovesText = datos.Moves.ToString("N0");
        SummaryOnHandText = datos.TotalStock.ToString("N2");

        HasSummary = true;
    }

    private async Task LoadMovesAsync()
    {
        var response = await _repository.GetAsync<List<ReportStockMoveDto>>($"{BaseUrl}/stock/moves{Filtro()}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Moves = new ObservableCollection<ReportStockMoveDto>(response.Response ?? new List<ReportStockMoveDto>());
    }

    //Las existencias son de HOY: solo les aplica la bodega, no el periodo
    private async Task LoadBalanceAsync()
    {
        var bodega = StorageId == Guid.Empty ? string.Empty : $"?storageId={StorageId}";

        var response = await _repository.GetAsync<List<ReportStockBalanceDto>>($"{BaseUrl}/stock/balance{bodega}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Balance = new ObservableCollection<ReportStockBalanceDto>(response.Response ?? new List<ReportStockBalanceDto>());
    }
}
