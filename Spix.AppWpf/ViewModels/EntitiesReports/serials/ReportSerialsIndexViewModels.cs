using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.serials;

// Inventario de seriales: cuantos equipos hay de cada producto y donde estan, o sea en la
// bodega, puestos en un cliente o averiados.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Por eso las filas
// no llevan botones y no hay ningun formulario detras.
//
// El reporte NO pagina: el backend devuelve una fila por producto de una sola vez, asi que
// no hay encabezado Totalpages que leer ni pie de paginacion que pintar.
public partial class ReportSerialsIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-inventory";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportSerialsRow> _rows = new();

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero: donde estan los equipos =====
    [ObservableProperty]
    private bool _hasSummary;

    //Los numeros del tablero viajan ya formateados porque el indicador solo sabe pintar
    //texto: si se le pasara el entero crudo saldria sin separador de miles
    [ObservableProperty]
    private string _summaryTotalText = string.Empty;

    [ObservableProperty]
    private string _summaryAvailableText = string.Empty;

    [ObservableProperty]
    private string _summaryOperativeText = string.Empty;

    [ObservableProperty]
    private string _summaryDamagedText = string.Empty;

    [ObservableProperty]
    private string _summaryProductsText = string.Empty;

    //===== La suma de la tabla, el renglon de totales de la web =====
    [ObservableProperty]
    private string _totalsText = string.Empty;

    public ReportSerialsIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Primero el tablero y despues la tabla, en serie: asi los numeros de arriba aparecen
    // antes, igual que en la web
    public async Task InitializeAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync();
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportSerialSummaryDto>($"{BaseUrl}/serials/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new ReportSerialSummaryDto();

        SummaryTotalText = datos.Total.ToString("N0");
        SummaryAvailableText = datos.Available.ToString("N0");
        SummaryOperativeText = datos.Operative.ToString("N0");
        SummaryDamagedText = datos.Damaged.ToString("N0");
        SummaryProductsText = $"{datos.Products:N0} productos con seriales";

        HasSummary = true;
    }

    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ReportSerialDto>>($"{BaseUrl}/serials");

            //Si falla se deja lo que hubiera en pantalla
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var datos = response.Response ?? new List<ReportSerialDto>();

            Rows = new ObservableCollection<ReportSerialsRow>(datos.Select(x => new ReportSerialsRow(x)));

            //La suma es de lo que se esta viendo, no del tablero: son dos cuentas distintas
            //y el pie tiene que hablar de la tabla
            TotalsText =
                $"Disponibles {datos.Sum(x => x.Available):N0}" +
                $"  ·  Instalados {datos.Sum(x => x.Operative):N0}" +
                $"  ·  Averiados {datos.Sum(x => x.Damaged):N0}" +
                $"  ·  Total {datos.Sum(x => x.Total):N0}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync();
    }
}

// Un producto con sus seriales, ya listo para pintar
public class ReportSerialsRow
{
    public ReportSerialDto Item { get; }

    public string ProductName => Item.ProductName;

    //Se guardan los numeros ademas del texto porque la tabla ordena por el numero:
    //ordenar por el texto pondria 9 despues de 100
    public int Available => Item.Available;

    public int Operative => Item.Operative;

    public int Damaged => Item.Damaged;

    public int Total => Item.Total;

    public string AvailableText => Item.Available.ToString("N0");

    public string OperativeText => Item.Operative.ToString("N0");

    public string DamagedText => Item.Damaged.ToString("N0");

    public string TotalText => Item.Total.ToString("N0");

    public ReportSerialsRow(ReportSerialDto item)
    {
        Item = item;
    }
}
