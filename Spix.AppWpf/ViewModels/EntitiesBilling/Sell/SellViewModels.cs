using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesBilling.Sell;
using Spix.Domain.EntitiesBilling;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using SellEntity = Spix.Domain.EntitiesBilling.Sell;

namespace Spix.AppWpf.ViewModels.EntitiesBilling.Sell;

// Facturas: lo que se le facturo a cada cliente, plan mensual y solicitudes de servicio.
//
// Es de SOLA CONSULTA: no crea, no edita, no anula y no imprime. La factura NACE en el
// lanzamiento de la facturacion (Notas de cobro o Nota individual); aqui solo se mira.
//
// El tablero es del MES EN CURSO y la tabla es TODO el historico, asi que los numeros de
// arriba no tienen por que cuadrar con los de abajo. Es asi tambien en la web.
public partial class SellIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/sells";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;

    [ObservableProperty]
    private ObservableCollection<SellRow> _rows = new();

    [ObservableProperty]
    private string? _filter;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero del mes =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _monthCount;

    [ObservableProperty]
    private decimal _monthTotal;

    [ObservableProperty]
    private int _monthPaid;

    [ObservableProperty]
    private int _monthCancelled;

    public SellIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
    }

    // Primero el tablero y despues la tabla, en serie: asi los numeros de arriba aparecen
    // antes, igual que en la web
    public async Task InitializeAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(1);
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<SellSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new SellSummaryDto();

        MonthCount = datos.MonthCount;
        MonthTotal = datos.MonthTotal;
        MonthPaid = datos.MonthPaid;
        MonthCancelled = datos.MonthCancelled;
        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            var response = await _repository.GetAsync<List<SellEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<SellRow>(
                (response.Response ?? new List<SellEntity>()).Select(x => new SellRow(x)));

            CurrentPage = page;

            //El total de paginas viaja en el encabezado
            if (response.HttpResponseMessage is not null &&
                response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
                int.TryParse(valores.FirstOrDefault(), out var total))
            {
                TotalPages = total;
            }
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
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    //Buscar vuelve a la primera pagina: en la web se queda marcando la pagina vieja y eso
    //despista, aqui la marca sigue a lo que se esta viendo
    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync(1);
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync(1);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }

    // El detalle no vuelve a pedir nada: usa la factura que ya trajo el listado
    [RelayCommand]
    private async Task DetailAsync(SellRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Sell"] = fila.Item
        };

        await _modalService.ShowAsync<SellDetailDialogView>(
            $"Detalle de la factura {fila.InvoiceNumber}", parametros);
    }
}

// Una fila de la tabla, ya lista para pintar
public class SellRow
{
    public SellEntity Item { get; }

    public string? InvoiceNumber => Item.InvoiceNumber;

    public string DateSellText => Item.DateSell.ToString("dd/MM/yyyy");

    public string ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public decimal SubTotal => Item.SubTotal;

    public decimal TotalTax => Item.TotalTax;

    public decimal Total => Item.Total;

    // El estado se decide en cascada: anulada manda sobre pagada, y pagada sobre pendiente
    public string StatusText => Item.Cancelled ? "Anulada" : Item.Paid ? "Pagada" : "Pendiente";

    public Brush StatusColor => Application.Current.TryFindResource(
        Item.Cancelled ? "BrushCatalogKpiBad" : Item.Paid ? "BrushCatalogKpiOk" : "BrushCatalogKpiWarn") as Brush
        ?? Brushes.Gray;

    public SellRow(SellEntity item)
    {
        Item = item;
    }
}

// El detalle de la factura: de que se compone. No pide nada al servidor.
public partial class SellDetailDialogViewModel : ObservableObject
{
    private readonly ModalService _modalService;

    [ObservableProperty]
    private ObservableCollection<SellLineRow> _lines = new();

    [ObservableProperty]
    private string? _clientFullName;

    [ObservableProperty]
    private string? _headerText;

    [ObservableProperty]
    private string? _invoiceNumber;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _totalTax;

    [ObservableProperty]
    private decimal _total;

    public SellDetailDialogViewModel(ModalService modalService)
    {
        _modalService = modalService;
    }

    public void Initialize(SellEntity sell)
    {
        ClientFullName = sell.ClientFullName;
        HeaderText = $"Contrato #{sell.ControlContrato} · {sell.DateSell:dd/MM/yyyy}";
        InvoiceNumber = sell.InvoiceNumber;

        Lines = new ObservableCollection<SellLineRow>(
            (sell.SellDetails ?? new List<SellDetail>()).Select(x => new SellLineRow(x)));

        //Los totales SI son los de la factura completa, con cantidad
        SubTotal = sell.SubTotal;
        TotalTax = sell.TotalTax;
        Total = sell.Total;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Un renglon de la factura
public class SellLineRow
{
    public SellDetail Item { get; }

    public string? Concept => Item.Concept;

    // El backend escribe estas dos cadenas y nada mas; cualquier otra se pinta tal cual
    public string OriginName => Item.Origin switch
    {
        "Plan" => "Plan",
        "SolicitudServicio" => "Servicio tecnico",
        null => string.Empty,
        _ => Item.Origin
    };

    //Los renglones muestran el valor UNITARIO, igual que en la web
    public decimal UnitPrice => Item.UnitPrice;

    public decimal TaxAmount => Item.TaxAmount;

    public decimal Price => Item.Price;

    public SellLineRow(SellDetail item)
    {
        Item = item;
    }
}
