using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Session;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesPayment.CxCBill;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CxCBillEntity = Spix.Domain.EntitiesPayment.CxCBill;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;

// Cuentas por cobrar: la caja del ISP.
//
// La nota NO nace aqui: nace con la factura cuando se lanza la facturacion del mes, y ya
// viene con el pago adelantado y la exoneracion aplicados. Aqui se hacen cuatro cosas:
// listar lo que se debe, explicar de que se compone una nota, recibir el dinero y anular.
//
// El recaudo es SIEMPRE total: no existen abonos parciales. El servicio calcula
// pago = deuda - descuento, y el saldo queda en cero.
//
// Este modulo NO toca el MikroTik. Cuando se cobra una nota de un contrato cortado, solo se
// le marca que ya pago: el equipo se toca una sola vez, desde Reactivacion y por servidor.
public partial class CxCBillIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/cxcbills";
    private const int PageSize = 15;

    //Desde dos letras, que es el corte del buscador de clientes de este modulo
    private const int MinimoParaBuscar = 2;
    private const int PausaMs = 500;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly IUserSessionService _sessionService;

    [ObservableProperty]
    private ObservableCollection<CxCBillRow> _rows = new();

    //===== El buscador de clientes =====
    [ObservableProperty]
    private string? _contractFilter;

    [ObservableProperty]
    private ObservableCollection<BillingContractDto> _contracts = new();

    private Guid? _selectedContractId;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _openNotes;

    [ObservableProperty]
    private decimal _openBalance;

    [ObservableProperty]
    private int _debtors;

    [ObservableProperty]
    private decimal _collectedMonth;

    private CancellationTokenSource? _cts;

    // Anular es de oficina: el tecnico cobra pero no anula. El backend tambien lo rechaza,
    // esto solo evita que el boton este ahi para quien no puede usarlo.
    public bool CanCancel =>
        string.Equals(_sessionService.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(_sessionService.Role, "Auxiliar", StringComparison.OrdinalIgnoreCase);

    public CxCBillIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService,
        IUserSessionService sessionService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
        _sessionService = sessionService;
    }

    public async Task InitializeAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(1);
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<CxCBillSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new CxCBillSummaryDto();

        OpenNotes = datos.OpenNotes;
        OpenBalance = datos.OpenBalance;
        Debtors = datos.Debtors;
        CollectedMonth = datos.CollectedMonth;
        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";

            if (_selectedContractId.HasValue)
            {
                url += $"&guidid={_selectedContractId.Value}";
            }

            var response = await _repository.GetAsync<List<CxCBillEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<CxCBillRow>(
                (response.Response ?? new List<CxCBillEntity>()).Select(x => new CxCBillRow(x, CanCancel)));

            CurrentPage = page;

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

    // Busca clientes desde dos letras. Admite varias a la vez para no perder teclas.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchContractsAsync(string? texto)
    {
        ContractFilter = texto;

        _cts?.Cancel();

        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < MinimoParaBuscar)
        {
            Contracts = new ObservableCollection<BillingContractDto>();
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(PausaMs, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        var response = await _repository.GetAsync<List<BillingContractDto>>(
            $"{BaseUrl}/searchclients?filter={Uri.EscapeDataString(texto.Trim())}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        if (await _responseHandler.HandleErrorAsync(response))
        {
            Contracts = new ObservableCollection<BillingContractDto>();
            return;
        }

        Contracts = new ObservableCollection<BillingContractDto>(
            response.Response ?? new List<BillingContractDto>());
    }

    [RelayCommand]
    private async Task SelectContractAsync(BillingContractDto? contrato)
    {
        if (contrato is null)
        {
            return;
        }

        _selectedContractId = contrato.ContractClientId;
        ContractFilter = $"{contrato.ClientFullName} - Contrato {contrato.ControlContrato}";
        Contracts = new ObservableCollection<BillingContractDto>();

        await LoadAsync(1);
    }

    [RelayCommand]
    private async Task ClearContractAsync()
    {
        _cts?.Cancel();
        _selectedContractId = null;
        ContractFilter = string.Empty;
        Contracts = new ObservableCollection<BillingContractDto>();

        await LoadAsync(1);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }

    // El detalle explica de que se compone la nota. No recarga nada al cerrarse.
    [RelayCommand]
    private async Task DetailAsync(CxCBillRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCBillId
        };

        await _modalService.ShowAsync<CxCBillDetailDialogView>($"Detalle {fila.CollectionNote}", parametros);
    }

    // Recibir el dinero. Es lo unico que mueve plata en esta pantalla.
    [RelayCommand]
    private async Task PayAsync(CxCBillRow? fila)
    {
        if (fila is null || !fila.CanPay)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCBillId
        };

        var result = await _modalService.ShowAsync<CxCBillPayDialogView>("Registrar Pago", parametros);

        if (result.Succeeded)
        {
            await LoadSummaryAsync();
            await LoadAsync(CurrentPage);
        }
    }

    // Anular NO borra: la nota queda como historia, y el pago adelantado y la exoneracion
    // vuelven a quedar sin facturar para que el periodo se pueda volver a facturar.
    [RelayCommand]
    private async Task CancelAsync(CxCBillRow? fila)
    {
        if (fila is null || !fila.CanCancel)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCBillId
        };

        var result = await _modalService.ShowAsync<CxCBillCancelDialogView>("Anular Cuenta", parametros);

        if (result.Succeeded)
        {
            await LoadSummaryAsync();
            await LoadAsync(CurrentPage);
        }
    }

    // El comprobante va por correo, en el cuerpo del mensaje: no hay PDF ni adjunto
    [RelayCommand]
    private async Task ReceiptAsync(CxCBillRow? fila)
    {
        if (fila is null || !fila.CanSendReceipt)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.PostAsync($"{BaseUrl}/{fila.CxCBillId}/receipt", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Enviar comprobante", "El comprobante se envio al correo del cliente.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// Una fila de la tabla, ya lista para pintar
public class CxCBillRow
{
    public CxCBillEntity Item { get; }

    public Guid CxCBillId => Item.CxCBillId;

    public string? CollectionNote => Item.CollectionNote;

    public string ClientName => $"{Item.Client?.FirstName} {Item.Client?.LastName}".Trim();

    public string NoteText =>
        $"{Item.CollectionNote} · Contrato #{Item.ContractClient?.ControlContrato} · {Item.DateNote:dd/MM/yyyy}";

    public decimal Total => Item.Total;

    public decimal TotalPayment => Item.TotalPayment;

    public decimal TotalDiscount => Item.TotalDiscount;

    public decimal Balance => Item.Balance;

    // Anulada manda sobre pagada, y pagada sobre pendiente
    public string StatusText => Item.Cancelled ? "Anulada" : Item.Paid ? "Pagada" : "Pendiente";

    public Brush StatusColor => Application.Current.TryFindResource(
        Item.Cancelled ? "BrushCatalogKpiBad" : Item.Paid ? "BrushCatalogKpiOk" : "BrushCatalogKpiWarn") as Brush
        ?? Brushes.Gray;

    //Cobrar solo lo que sigue debiendo
    public bool CanPay => !Item.Paid && !Item.Cancelled && Item.Balance > 0;

    //El comprobante solo tiene sentido si ya entro plata
    public bool CanSendReceipt => Item.TotalPayment > 0;

    //Anular: solo la oficina, y solo mientras no este pagada ni anulada
    public bool CanCancel { get; }

    public CxCBillRow(CxCBillEntity item, bool puedeAnular)
    {
        Item = item;
        CanCancel = puedeAnular && !item.Paid && !item.Cancelled;
    }
}
