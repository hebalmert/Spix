using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;

// Los cinco modales de la cuenta por pagar: armarla, ver sus comisiones, ver sus abonos,
// abonar y anular.

// Armar la cuenta: se elige el contratista y se marcan las comisiones que se le agrupan.
public partial class CreateContractorCxCDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractor-payments";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;

    //La lista la arma el backend: solo trae contratistas CON comisiones pendientes, y su
    //elemento neutro en la posicion 0
    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _contractors = new();

    [ObservableProperty]
    private Guid _contractorId;

    [ObservableProperty]
    private ObservableCollection<PendingCommissionRow> _pending = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    public bool HasPending => Pending.Count > 0;

    //Solo se ve el aviso cuando ya se eligio contratista y no tiene nada pendiente
    public bool ShowNothingPending => ContractorId != Guid.Empty && !IsLoading && Pending.Count == 0;

    public int SelectedCount => Pending.Count(x => x.IsSelected);

    public string SelectedText => $"{SelectedCount} seleccionadas";

    public decimal Total => Pending.Where(x => x.IsSelected).Sum(x => x.Total);

    public CreateContractorCxCDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
    }

    public async Task InitializeAsync()
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combocontractors");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Contractors = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    // Al cambiar de contratista se bajan SUS comisiones pendientes: las de antes ya no valen
    partial void OnContractorIdChanged(Guid value) => _ = CargarPendientesAsync();

    private async Task CargarPendientesAsync()
    {
        Pending = new ObservableCollection<PendingCommissionRow>();
        RefrescarTotales();

        if (ContractorId == Guid.Empty)
        {
            OnPropertyChanged(nameof(ShowNothingPending));
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<ContractorPendingDto>>(
                $"{BaseUrl}/pending/{ContractorId}");

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var filas = (response.Response ?? new List<ContractorPendingDto>())
                .Select(x => new PendingCommissionRow(x, RefrescarTotales));

            Pending = new ObservableCollection<PendingCommissionRow>(filas);
        }
        finally
        {
            IsLoading = false;
            RefrescarTotales();
        }
    }

    private void RefrescarTotales()
    {
        OnPropertyChanged(nameof(HasPending));
        OnPropertyChanged(nameof(ShowNothingPending));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(SelectedText));
        OnPropertyChanged(nameof(Total));
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var fila in Pending)
        {
            fila.IsSelected = true;
        }

        RefrescarTotales();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        //Sin contratista o sin nada marcado no hay cuenta que armar
        if (ContractorId == Guid.Empty || SelectedCount == 0)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var datos = new CxCContractorCreateDto
            {
                ContractorId = ContractorId,
                ContractorAccountPayableIds = Pending
                    .Where(x => x.IsSelected)
                    .Select(x => x.ContractorAccountPayableId)
                    .ToList()
            };

            var response = await _repository.PostAsync($"{BaseUrl}/cxc", datos);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

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

// Una comision pendiente de agrupar. Es la unica fila que se puede marcar, por eso lleva
// notificacion propia: el total de arriba tiene que moverse al tocar la casilla.
public partial class PendingCommissionRow : ObservableObject
{
    private readonly Action _alMarcar;

    public ContractorPendingDto Item { get; }

    [ObservableProperty]
    private bool _isSelected;

    public Guid ContractorAccountPayableId => Item.ContractorAccountPayableId;

    public string? ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public string? CollectionNote => Item.CollectionNote;

    public string DateCreatedText => Item.DateCreated.ToString("dd/MM/yyyy");

    public decimal BaseAmount => Item.BaseAmount;

    public string RateText => $"{Item.Rate.ToString("N2", CultureInfo.CurrentCulture)}%";

    public decimal Total => Item.Total;

    public PendingCommissionRow(ContractorPendingDto item, Action alMarcar)
    {
        Item = item;
        _alMarcar = alMarcar;
    }

    partial void OnIsSelectedChanged(bool value) => _alMarcar();
}

// La cabecera que comparten los tres modales que miran una cuenta ya armada
public abstract partial class ContractorCxCDocumentViewModel : ObservableObject
{
    protected const string BaseUrl = "api/v1/contractor-payments";
    protected const int PageSize = 10;

    protected readonly IRepository Repository;
    protected readonly HttpResponseHandler ResponseHandler;
    protected readonly ModalService Modal;

    [ObservableProperty]
    private string? _contractorName;

    [ObservableProperty]
    private string? _noteNumber;

    [ObservableProperty]
    private string? _dateNoteText;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _balance;

    [ObservableProperty]
    private bool _isLoading;

    protected Guid Id;

    //Lo abonado no viaja: se deduce de lo que ya no se debe
    public decimal Paid => Total - Balance;

    protected ContractorCxCDocumentViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modal)
    {
        Repository = repository;
        ResponseHandler = responseHandler;
        Modal = modal;
    }

    public void SetId(Guid id) => Id = id;

    protected async Task<bool> CargarCabeceraAsync()
    {
        var response = await Repository.GetAsync<CxCContractor>($"{BaseUrl}/cxc/{Id}");
        if (await ResponseHandler.HandleErrorAsync(response))
        {
            await Modal.CloseAsync(ModalResult.Cancel());
            return false;
        }

        var nota = response.Response;

        if (nota is null)
        {
            await Modal.CloseAsync(ModalResult.Cancel());
            return false;
        }

        ContractorName = $"{nota.Contractor?.FirstName} {nota.Contractor?.LastName}".Trim();
        NoteNumber = nota.NoteNumber;
        DateNoteText = nota.DateNote.ToString("dd/MM/yyyy");
        Total = nota.Total;
        Balance = nota.Balance;

        OnPropertyChanged(nameof(Paid));

        return true;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Modal.CloseAsync(ModalResult.Cancel());
    }
}

// De que se compone la cuenta: las comisiones que se le agruparon
public partial class ContractorCxCCommissionsDialogViewModel : ContractorCxCDocumentViewModel
{
    [ObservableProperty]
    private ObservableCollection<CommissionRow> _rows = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    public ContractorCxCCommissionsDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modal)
        : base(repository, responseHandler, modal)
    {
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            if (!await CargarCabeceraAsync())
            {
                return;
            }

            await CargarAsync(1);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarAsync(int page)
    {
        var response = await Repository.GetAsync<List<ContractorPendingDto>>(
            $"{BaseUrl}/cxc/{Id}/commissions?page={page}&recordsnumber={PageSize}");

        if (await ResponseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<CommissionRow>(
            (response.Response ?? new List<ContractorPendingDto>()).Select(x => new CommissionRow(x)));

        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await CargarAsync(page);
    }
}

// Una comision ya agrupada. Aqui el Total es el valor original de la comision, no el saldo:
// despues de pagar la cuenta el saldo queda en cero y se perderia el dato.
public class CommissionRow
{
    public ContractorPendingDto Item { get; }

    public string? ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public string? CollectionNote => Item.CollectionNote;

    public string DateCreatedText => Item.DateCreated.ToString("dd/MM/yyyy");

    public decimal BaseAmount => Item.BaseAmount;

    public string RateText => $"{Item.Rate.ToString("N2", CultureInfo.CurrentCulture)}%";

    public decimal Total => Item.Total;

    public CommissionRow(ContractorPendingDto item)
    {
        Item = item;
    }
}

// Los abonos que se le han hecho a la cuenta
public partial class ContractorCxCPaymentsDialogViewModel : ContractorCxCDocumentViewModel
{
    [ObservableProperty]
    private ObservableCollection<ContractorPaymentRow> _rows = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    public ContractorCxCPaymentsDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modal)
        : base(repository, responseHandler, modal)
    {
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            if (!await CargarCabeceraAsync())
            {
                return;
            }

            await CargarAsync(1);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CargarAsync(int page)
    {
        var response = await Repository.GetAsync<List<CxCContractorPaymentItemDto>>(
            $"{BaseUrl}/cxc/{Id}/payments?page={page}&recordsnumber={PageSize}");

        if (await ResponseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<ContractorPaymentRow>(
            (response.Response ?? new List<CxCContractorPaymentItemDto>()).Select(x => new ContractorPaymentRow(x)));

        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await CargarAsync(page);
    }
}

// Un abono. El saldo es el que quedo DESPUES de ese abono.
public class ContractorPaymentRow
{
    public CxCContractorPaymentItemDto Item { get; }

    public string DatePaymentText => Item.DatePayment.ToString("dd/MM/yyyy");

    public string? UsuarioOwner => Item.UsuarioOwner;

    public string ModeName => Item.PaymentMode switch
    {
        "Cash" => "Efectivo",
        "Card" => "Tarjeta",
        "Transfer" => "Transferencia",
        null => string.Empty,
        _ => Item.PaymentMode
    };

    public string? Reference => Item.Reference;

    public string? Detail => Item.Detail;

    public decimal Payment => Item.Payment;

    public decimal Balance => Item.Balance;

    public ContractorPaymentRow(CxCContractorPaymentItemDto item)
    {
        Item = item;
    }
}

// Abonarle al contratista. A diferencia del cobro al cliente, aqui SI hay abonos parciales.
public partial class ContractorCxCPayDialogViewModel : ContractorCxCDocumentViewModel
{
    private readonly AlertService _alertService;

    [ObservableProperty]
    private decimal _payment;

    [ObservableProperty]
    private string _paymentMode = "Cash";

    [ObservableProperty]
    private string? _reference;

    [ObservableProperty]
    private string? _detail;

    [ObservableProperty]
    private bool _isSaving;

    //Lo que quedaria debiendo despues de este abono
    public decimal Rest => Balance - Payment;

    public bool IsCash => PaymentMode == "Cash";

    public bool IsCard => PaymentMode == "Card";

    public bool IsTransfer => PaymentMode == "Transfer";

    public ContractorCxCPayDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modal,
        AlertService alertService)
        : base(repository, responseHandler, modal)
    {
        _alertService = alertService;

        //Balance vive en la clase base, asi que su aviso no se puede implementar aqui:
        //se engancha al evento para recalcular lo que quedaria debiendo
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Balance))
            {
                OnPropertyChanged(nameof(Rest));
            }
        };
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            if (!await CargarCabeceraAsync())
            {
                return;
            }

            //Se propone el saldo completo, que es lo normal
            Payment = Balance;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnPaymentChanged(decimal value)
    {
        //Nunca por encima del saldo ni por debajo de cero
        var ajustado = Math.Round(Math.Clamp(value, 0m, Balance), 2);

        if (ajustado != value)
        {
            Payment = ajustado;
            return;
        }

        OnPropertyChanged(nameof(Rest));
    }

    partial void OnPaymentModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsCash));
        OnPropertyChanged(nameof(IsCard));
        OnPropertyChanged(nameof(IsTransfer));
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
    private void SetFullBalance()
    {
        Payment = Balance;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Payment <= 0)
        {
            await _alertService.WarningAsync("Registrar pago", "El monto debe ser mayor que cero.");
            return;
        }

        IsSaving = true;

        try
        {
            var datos = new CxCContractorPaymentDto
            {
                CxCContractorId = Id,
                Payment = Payment,
                PaymentMode = PaymentMode,
                Reference = Reference,
                Detail = Detail
            };

            var response = await Repository.PostAsync($"{BaseUrl}/cxc/pay", datos);
            if (await ResponseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await Modal.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }
}

// Anular la cuenta. No pide nada al servidor: solo el motivo.
public partial class ContractorCxCCancelDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractor-payments";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private string? _motivo;

    [ObservableProperty]
    private bool _isSaving;

    private Guid _id;

    public string WarningText =>
        "Al anularla, sus comisiones vuelven a quedar pendientes y se pueden agrupar de nuevo.";

    public ContractorCxCCancelDialogViewModel(
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
        if (string.IsNullOrWhiteSpace(Motivo))
        {
            await _alertService.WarningAsync("Anular cuenta", "Debe especificar el motivo de la anulacion.");
            return;
        }

        IsSaving = true;

        try
        {
            //El motivo viaja como el cuerpo entero, no dentro de un objeto
            var response = await _repository.PostAsync($"{BaseUrl}/cxc/{_id}/cancel", Motivo);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

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
