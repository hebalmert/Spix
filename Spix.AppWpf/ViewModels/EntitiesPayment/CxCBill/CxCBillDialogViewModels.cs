using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Collections.ObjectModel;
using CxCBillEntity = Spix.Domain.EntitiesPayment.CxCBill;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;

// Los tres modales de la caja: ver la nota, recibir el dinero y anular.
// Van aparte del index porque cada uno tiene su propia carga y sus propias reglas.

// El detalle de la nota: de que se compone y que se le ha recibido.
public partial class CxCBillDetailDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/cxcbills";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;

    [ObservableProperty]
    private ObservableCollection<CxCSellLineRow> _lines = new();

    [ObservableProperty]
    private ObservableCollection<CxCBillPaymentRow> _payments = new();

    [ObservableProperty]
    private string? _clientName;

    [ObservableProperty]
    private string? _headerText;

    [ObservableProperty]
    private string? _collectionNote;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _totalPayment;

    [ObservableProperty]
    private decimal _totalDiscount;

    [ObservableProperty]
    private decimal _balance;

    [ObservableProperty]
    private bool _isLoading;

    private Guid _id;

    public CxCBillDetailDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
    }

    public void SetId(Guid id) => _id = id;

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<CxCBillEntity>($"{BaseUrl}/{_id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var nota = response.Response;

            if (nota is null)
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            ClientName = $"{nota.Client?.FirstName} {nota.Client?.LastName}".Trim();
            HeaderText = $"Contrato #{nota.ContractClient?.ControlContrato} - {nota.MonthType} {nota.YearNumber} - {nota.DateNote:dd/MM/yyyy}";
            CollectionNote = nota.CollectionNote;

            //De que se compone: son los renglones de la FACTURA, no de la nota
            Lines = new ObservableCollection<CxCSellLineRow>(
                (nota.Sell?.SellDetails ?? new List<SellDetail>()).Select(x => new CxCSellLineRow(x)));

            //Lo que se le ha recibido
            Payments = new ObservableCollection<CxCBillPaymentRow>(
                (nota.CxCBillDetails ?? new List<CxCBillDetail>()).Select(x => new CxCBillPaymentRow(x)));

            Total = nota.Total;
            TotalPayment = nota.TotalPayment;
            TotalDiscount = nota.TotalDiscount;
            Balance = nota.Balance;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Un renglon de la factura que respalda la nota
public class CxCSellLineRow
{
    public SellDetail Item { get; }

    public string? Concept => Item.Concept;

    public string OriginName => Item.Origin switch
    {
        "Plan" => "Plan",
        "SolicitudServicio" => "Servicio tecnico",
        null => string.Empty,
        _ => Item.Origin
    };

    public decimal UnitPrice => Item.UnitPrice;

    public decimal TaxAmount => Item.TaxAmount;

    public decimal Price => Item.Price;

    public CxCSellLineRow(SellDetail item)
    {
        Item = item;
    }
}

// Un abono recibido sobre la nota
public class CxCBillPaymentRow
{
    public CxCBillDetail Item { get; }

    public string DatePaymentText => Item.DatePayment.ToString("dd/MM/yyyy");

    public string ModeName => Item.PaymentMode switch
    {
        "Cash" => "Efectivo",
        "Card" => "Tarjeta",
        "Transfer" => "Transferencia",
        null => string.Empty,
        _ => Item.PaymentMode
    };

    public string? Detail => Item.Detail;

    public decimal Payment => Item.Payment;

    public decimal Discount => Item.Discount;

    public CxCBillPaymentRow(CxCBillDetail item)
    {
        Item = item;
    }
}

// Recibir el dinero de una nota.
//
// El recaudo es SIEMPRE total: se elige el modo de pago y, si aplica, un porcentaje de
// descuento; se cobra el resto y la nota queda en cero. No hay abonos parciales.
public partial class CxCBillPayDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/cxcbills";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string? _clientName;

    [ObservableProperty]
    private string? _headerText;

    [ObservableProperty]
    private string? _collectionNote;

    [ObservableProperty]
    private decimal _debt;

    [ObservableProperty]
    private string _paymentMode = "Cash";

    [ObservableProperty]
    private int _discountPercent;

    [ObservableProperty]
    private string? _detail;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _id;

    //El mismo calculo y el mismo redondeo que hace el servidor
    public decimal DiscountAmount => Math.Round((Debt * DiscountPercent) / 100, 2);

    public decimal PaymentAmount => Debt - DiscountAmount;

    //Siempre cero: el recaudo cancela la nota completa
    public decimal Balance => Debt - DiscountAmount - PaymentAmount;

    //Para pintar el chip elegido
    public bool IsCash => PaymentMode == "Cash";

    public bool IsCard => PaymentMode == "Card";

    public bool IsTransfer => PaymentMode == "Transfer";

    public bool IsDiscount0 => DiscountPercent == 0;

    public bool IsDiscount25 => DiscountPercent == 25;

    public bool IsDiscount50 => DiscountPercent == 50;

    public bool IsDiscount75 => DiscountPercent == 75;

    public bool IsDiscount100 => DiscountPercent == 100;

    public CxCBillPayDialogViewModel(
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

    public void SetId(Guid id) => _id = id;

    partial void OnDebtChanged(decimal value) => RefrescarCuentas();

    partial void OnDiscountPercentChanged(int value)
    {
        RefrescarCuentas();

        OnPropertyChanged(nameof(IsDiscount0));
        OnPropertyChanged(nameof(IsDiscount25));
        OnPropertyChanged(nameof(IsDiscount50));
        OnPropertyChanged(nameof(IsDiscount75));
        OnPropertyChanged(nameof(IsDiscount100));
    }

    partial void OnPaymentModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsCash));
        OnPropertyChanged(nameof(IsCard));
        OnPropertyChanged(nameof(IsTransfer));
    }

    private void RefrescarCuentas()
    {
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(PaymentAmount));
        OnPropertyChanged(nameof(Balance));
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<CxCBillEntity>($"{BaseUrl}/{_id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var nota = response.Response;

            if (nota is null)
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            ClientName = $"{nota.Client?.FirstName} {nota.Client?.LastName}".Trim();
            HeaderText = $"Contrato #{nota.ContractClient?.ControlContrato} - {nota.DateNote:dd/MM/yyyy}";
            CollectionNote = nota.CollectionNote;
            Debt = nota.Balance;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SetMode(string? modo)
    {
        if (!string.IsNullOrWhiteSpace(modo))
        {
            PaymentMode = modo;
        }
    }

    [RelayCommand]
    private void SetDiscount(string? porcentaje)
    {
        if (int.TryParse(porcentaje, out var valor))
        {
            DiscountPercent = valor;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        //Un descuento sin explicacion no se guarda: es plata que se deja de cobrar
        if (DiscountPercent > 0 && string.IsNullOrWhiteSpace(Detail))
        {
            await _alertService.WarningAsync("Registrar pago", "Debe especificar la razon del descuento.");
            return;
        }

        IsSaving = true;

        try
        {
            var datos = new CxCBillPaymentDto
            {
                CxCBillId = _id,
                PaymentMode = PaymentMode,
                DiscountPercent = DiscountPercent,
                Detail = Detail
            };

            var response = await _repository.PostAsync($"{BaseUrl}/pay", datos);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Registrado", "Pago registrado correctamente.");
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Anular una nota. NO borra: la deja como historia y devuelve el pago adelantado y la
// exoneracion al estado sin facturar, para que el periodo se pueda volver a facturar.
public partial class CxCBillCancelDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/cxcbills";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string? _descriptionCancelled;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _id;

    public CxCBillCancelDialogViewModel(
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

    public void SetId(Guid id) => _id = id;

    [RelayCommand]
    private async Task SaveAsync()
    {
        //El motivo es obligatorio: una anulacion sin explicacion no sirve de nada despues
        if (string.IsNullOrWhiteSpace(DescriptionCancelled))
        {
            await _alertService.WarningAsync("Validacion", "Debe especificar el motivo de anulacion.");
            return;
        }

        IsSaving = true;

        try
        {
            var datos = new CxCBillCancelDto
            {
                CxCBillId = _id,
                DescriptionCancelled = DescriptionCancelled
            };

            var response = await _repository.PostAsync($"{BaseUrl}/cancel", datos);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Anulado", "Cuenta por cobrar anulada correctamente.");
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
