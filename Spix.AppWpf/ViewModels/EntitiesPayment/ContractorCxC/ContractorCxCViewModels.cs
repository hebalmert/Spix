using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;

// Pagos a contratistas: lo que se le debe a cada uno por los pagos que recaudo.
//
// Es el espejo de la cuenta por cobrar del cliente, pero del lado de la plata que SALE.
// La comision no se crea aqui: se causa sola cuando se recauda una nota de cobro, si el
// contrato tiene un contratista con porcentaje. Esa comision nace suelta, pendiente de
// agrupar.
//
// Aqui se agrupan las comisiones pendientes de un contratista en una CUENTA POR PAGAR y
// despues se le abona contra ella, completo o por partes. Anular devuelve las comisiones
// a pendientes para poder agruparlas de nuevo.
//
// Este modulo NO toca el MikroTik.
public partial class ContractorCxCIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractor-payments";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<ContractorCxCRow> _rows = new();

    [ObservableProperty]
    private string? _filter;

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
    private int _pending;

    [ObservableProperty]
    private decimal _pendingTotal;

    [ObservableProperty]
    private int _openNotes;

    [ObservableProperty]
    private decimal _openBalance;

    public ContractorCxCIndexViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(1);
    }

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<CxCContractorSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new CxCContractorSummaryDto();

        Pending = datos.Pending;
        PendingTotal = datos.PendingTotal;
        OpenNotes = datos.OpenNotes;
        OpenBalance = datos.OpenBalance;
        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}/cxc?page={page}&recordsnumber={PageSize}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            var response = await _repository.GetAsync<List<CxCContractor>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ContractorCxCRow>(
                (response.Response ?? new List<CxCContractor>()).Select(x => new ContractorCxCRow(x)));

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

    //Recargar el tablero y la MISMA pagina: despues de abonar el usuario sigue donde estaba
    private async Task RecargarAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await RecargarAsync();
    }

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

    // Armar una cuenta: se elige el contratista y se marcan sus comisiones pendientes
    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateContractorCxCDialogView>("Nueva cuenta por pagar");

        if (result.Succeeded)
        {
            await RecargarAsync();
            await _alertService.SuccessAsync("Pagos a contratistas", "Cuenta creada.");
        }
    }

    // De que se compone la cuenta: las comisiones que se agruparon
    [RelayCommand]
    private async Task CommissionsAsync(ContractorCxCRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCContractorId
        };

        await _modalService.ShowAsync<ContractorCxCCommissionsDialogView>("Comisiones de la cuenta", parametros);
    }

    // Lo que ya se le ha abonado contra esta cuenta
    [RelayCommand]
    private async Task PaymentsAsync(ContractorCxCRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCContractorId
        };

        await _modalService.ShowAsync<ContractorCxCPaymentsDialogView>("Abonos de la cuenta", parametros);
    }

    // Abonarle: puede ser el saldo completo o una parte
    [RelayCommand]
    private async Task PayAsync(ContractorCxCRow? fila)
    {
        if (fila is null || !fila.CanPay)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCContractorId
        };

        var result = await _modalService.ShowAsync<ContractorCxCPayDialogView>("Registrar pago", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
            await _alertService.SuccessAsync("Pagos a contratistas", "Pago registrado.");
        }
    }

    // Anular devuelve las comisiones a pendientes: se pueden volver a agrupar
    [RelayCommand]
    private async Task CancelAsync(ContractorCxCRow? fila)
    {
        if (fila is null || !fila.CanPay)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.CxCContractorId
        };

        var result = await _modalService.ShowAsync<ContractorCxCCancelDialogView>("Anular cuenta", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();

            //La web avisa aqui con el texto de confirmacion de borrado, que no dice nada de
            //lo que paso. En el escritorio se dice lo que realmente ocurrio.
            await _alertService.SuccessAsync("Pagos a contratistas",
                "Cuenta anulada. Sus comisiones volvieron a quedar pendientes.");
        }
    }
}

// Una fila de la tabla, ya lista para pintar
public class ContractorCxCRow
{
    public CxCContractor Item { get; }

    public Guid CxCContractorId => Item.CxCContractorId;

    public string? NoteNumber => Item.NoteNumber;

    public string DateNoteText => Item.DateNote.ToString("dd/MM/yyyy");

    public string ContractorName => $"{Item.Contractor?.FirstName} {Item.Contractor?.LastName}".Trim();

    public decimal Total => Item.Total;

    public decimal Balance => Item.Balance;

    // Anulada manda sobre pagada, y pagada sobre abierta
    public string StatusText => Item.Cancelled ? "Anulada" : Item.Paid ? "Pagada" : "Abierta";

    public Brush StatusColor => Application.Current.TryFindResource(
        Item.Cancelled ? "BrushCatalogKpiBad" : Item.Paid ? "BrushCatalogKpiOk" : "BrushCatalogKpiWarn") as Brush
        ?? Brushes.Gray;

    //Abonar y anular piden lo mismo: que siga abierta y con saldo
    public bool CanPay => !Item.Paid && !Item.Cancelled && Item.Balance > 0;

    public ContractorCxCRow(CxCContractor item)
    {
        Item = item;
    }
}
