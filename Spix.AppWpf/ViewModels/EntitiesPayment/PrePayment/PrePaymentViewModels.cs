using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesPayment.PrePayment;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using PrePaymentEntity = Spix.Domain.EntitiesPayment.PrePayment;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.PrePayment;

// Pagos adelantados: dinero que el cliente entrega ANTES de que exista la nota de cobro.
//
// Cada registro es un contrato con un ano y un mes, y guarda congelado el plan de ese mes
// mas los servicios tecnicos ya ejecutados que el cliente decide pagar de una vez. Los
// servicios marcados quedan RESERVADOS por este pago: no se facturan aqui, solo se apartan.
//
// Cuando la facturacion genera la nota de cobro de ese contrato para ese mismo periodo,
// encuentra el adelanto, lo aplica como abono y lo marca facturado. Por eso el listado solo
// trae los NO facturados: al cruzarse, la fila desaparece de esta pantalla.
public partial class PrePaymentIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/prepayments";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    //El nombre del mes lo traduce el backend; aqui solo se busca por su numero
    private readonly Dictionary<int, string> _meses = new();

    [ObservableProperty]
    private ObservableCollection<PrePaymentRow> _rows = new();

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
    private int _contracts;

    [ObservableProperty]
    private decimal _billedMonthTotal;

    public PrePaymentIndexViewModel(
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

    // Los meses se bajan primero: la columna Periodo los necesita para escribir el nombre
    public async Task InitializeAsync()
    {
        await LoadMonthsAsync();
        await ReloadAsync();
    }

    private async Task LoadMonthsAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        _meses.Clear();

        foreach (var item in response.Response ?? new List<IntItemModel>())
        {
            _meses[item.Value] = item.Name ?? string.Empty;
        }
    }

    // El tablero cuesta tres consultas: se recarga solo cuando algo cambio de verdad
    private async Task ReloadAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<PrePaymentSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new PrePaymentSummaryDto();

        Pending = datos.Pending;
        PendingTotal = datos.PendingTotal;
        Contracts = datos.Contracts;
        BilledMonthTotal = datos.BilledMonthTotal;
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

            var response = await _repository.GetAsync<List<PrePaymentEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<PrePaymentRow>(
                (response.Response ?? new List<PrePaymentEntity>())
                    .Select(x => new PrePaymentRow(x, NombreDelMes(x.MonthType))));

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

    //Si la carga de meses fallo queda el nombre del enum, igual que en la web
    private string NombreDelMes(MonthType mes)
    {
        return _meses.TryGetValue((int)mes, out var nombre) && !string.IsNullOrWhiteSpace(nombre)
            ? nombre
            : mes.ToString();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await ReloadAsync();
    }

    //Buscar y paginar NO recargan el tablero: son los mismos numeros y costarian tres
    //consultas mas por cada tecla
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

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<PrePaymentDialogView>("Nuevo pago adelantado");

        if (!result.Succeeded)
        {
            return;
        }

        await ReloadAsync();
        await _alertService.SuccessAsync("Registrado", "El Registro se realizo con exito.");
    }

    // Editar no avisa nada al terminar: es asi tambien en la web
    [RelayCommand]
    private async Task EditAsync(PrePaymentRow? item)
    {
        if (item is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = item.PrePaymentId
        };

        var result = await _modalService.ShowAsync<PrePaymentDialogView>(
            "Editar pago adelantado", parametros);

        if (result.Succeeded)
        {
            await ReloadAsync();
        }
    }

    // Borrar libera los servicios reservados: por eso se avisa antes
    [RelayCommand]
    private async Task DeleteAsync(PrePaymentRow? item)
    {
        if (item is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Borrado",
            "Se borrara el pago adelantado y sus servicios quedaran libres.",
            "Si, Borralo");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.DeleteAsync($"{BaseUrl}/{item.PrePaymentId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }
        }
        finally
        {
            IsLoading = false;
        }

        await ReloadAsync();
        await _alertService.SuccessAsync("Borrado", "El registro se borro con exito.");
    }
}

// Una fila de la tabla, ya lista para pintar
public class PrePaymentRow
{
    public PrePaymentEntity Item { get; }

    public Guid PrePaymentId => Item.PrePaymentId;

    public string ClientName => $"{Item.Client?.FirstName} {Item.Client?.LastName}".Trim();

    public string ContractText => $"Contrato #{Item.ContractClient?.ControlContrato}";

    public string PeriodText { get; }

    public string? PlanName => Item.Plan?.PlanName;

    //Solo las lineas de servicio; la del plan la lleva todo pago adelantado
    public int ServiceLines => Item.PrePaymentDetails?.Count(x => x.ServiceRequestDetailId.HasValue) ?? 0;

    public bool HasServices => ServiceLines > 0;

    public string ServicesText => $"+ {ServiceLines} servicio(s)";

    public string DatePaymentText => Item.DatePayment.ToString("dd/MM/yyyy");

    public decimal PriceWithTax => Item.PriceWithTax;

    public PrePaymentRow(PrePaymentEntity item, string periodText)
    {
        Item = item;
        PeriodText = $"{periodText} {item.YearNumber}";
    }
}

// Registrar o corregir un pago adelantado: el mismo formulario para los dos, porque en la
// web tambien es el mismo (Create y Edit solo son el cascaron del modal).
//
// Sin Id se crea; con Id se edita. Al editar se piden los servicios pasando el Id del pago,
// para que los que este mismo pago tiene reservados vuelvan a salir como disponibles.
public partial class PrePaymentDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/prepayments";

    //Dos letras, que es lo que exige el backend, y pausa antes de consultar
    private const int MinimoParaBuscar = 2;
    private const int PausaMs = 500;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    //Los servicios que el pago ya tenia reservados, para marcarlos cuando lleguen
    private readonly HashSet<Guid> _reservados = new();

    private CancellationTokenSource? _cts;

    private Guid _prePaymentId;

    private string? _paymentControl;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private ObservableCollection<BillingContractDto> _candidates = new();

    [ObservableProperty]
    private ObservableCollection<PrePaymentServiceRow> _services = new();

    [ObservableProperty]
    private BillingContractDto? _selectedContract;

    [ObservableProperty]
    private Guid _contractClientId;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private DateTime? _datePayment = DateTime.Today;

    [ObservableProperty]
    private int _yearNumber;

    [ObservableProperty]
    private decimal _servicesTotal;

    [ObservableProperty]
    private bool _isEdit;

    [ObservableProperty]
    private bool _billed;

    [ObservableProperty]
    private string? _dateBilledText;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _noResults;

    [ObservableProperty]
    private bool _servicesLoading;

    private MonthType _monthType = MonthType.January;

    // El combo trabaja con int y el modelo con el enum
    public int MonthValue
    {
        get => (int)_monthType;
        set
        {
            //El combo trae "[Seleccione Mes]" con valor 0: elegirlo no cambia el mes, igual
            //que la guarda que tiene la web
            if (!Enum.IsDefined(typeof(MonthType), value))
            {
                OnPropertyChanged();
                return;
            }

            _monthType = (MonthType)value;
            OnPropertyChanged();
        }
    }

    public bool HasContract => ContractClientId != Guid.Empty;

    //Con contrato elegido y la lista ya traida, decirlo cuando no hay nada que adelantar
    public bool NoServices => HasContract && !ServicesLoading && Services.Count == 0;

    public decimal PlanTotal => SelectedContract?.PlanPriceWithTax ?? 0;

    public decimal Total => PlanTotal + ServicesTotal;

    public PrePaymentDialogViewModel(
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

    partial void OnSelectedContractChanged(BillingContractDto? value)
    {
        OnPropertyChanged(nameof(PlanTotal));
        OnPropertyChanged(nameof(Total));
    }

    partial void OnContractClientIdChanged(Guid value)
    {
        OnPropertyChanged(nameof(HasContract));
        OnPropertyChanged(nameof(NoServices));
    }

    partial void OnServicesChanged(ObservableCollection<PrePaymentServiceRow> value)
    {
        OnPropertyChanged(nameof(NoServices));
    }

    partial void OnServicesLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(NoServices));
    }

    partial void OnServicesTotalChanged(decimal value)
    {
        OnPropertyChanged(nameof(Total));
    }

    // Guid.Empty = pago nuevo; con Id se abre el que se va a corregir
    public async Task InitializeAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            await LoadMonthsAsync();

            if (id == Guid.Empty)
            {
                AplicarPeriodoPorDefecto();
                return;
            }

            IsEdit = true;
            await LoadModelAsync(id);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMonthsAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Months = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    //El adelanto se recibe para el mes que viene: es el caso de todos los dias
    private void AplicarPeriodoPorDefecto()
    {
        var siguiente = DateTime.Today.AddMonths(1);

        DatePayment = DateTime.Today;
        YearNumber = siguiente.Year;
        MonthValue = siguiente.Month;
    }

    private async Task LoadModelAsync(Guid id)
    {
        var response = await _repository.GetAsync<PrePaymentEntity>($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        var model = response.Response;
        if (model is null)
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        _prePaymentId = model.PrePaymentId;
        _paymentControl = model.PaymentControl;

        DatePayment = model.DatePayment;
        YearNumber = model.YearNumber;
        MonthValue = (int)model.MonthType;
        Billed = model.Billed;
        DateBilledText = model.DateBilled?.ToString("dd/MM/yyyy");

        SelectedContract = ConstruirContrato(model);
        SearchText = SelectedContract?.ClientFullName;
        ContractClientId = model.ContractClientId;

        _reservados.Clear();

        foreach (var linea in model.PrePaymentDetails ?? new List<PrePaymentDetail>())
        {
            if (linea.ServiceRequestDetailId.HasValue)
            {
                _reservados.Add(linea.ServiceRequestDetailId.Value);
            }
        }

        await LoadServicesAsync(model.ContractClientId);
    }

    // El encabezado del formulario, armado con lo que quedo guardado en el pago.
    //
    // El precio que se muestra es el de la LINEA DEL PLAN, no el total del pago: el total
    // ya incluye los servicios, y volver a sumarlos daria un total inflado.
    private static BillingContractDto? ConstruirContrato(PrePaymentEntity model)
    {
        var contrato = model.ContractClient;
        if (contrato is null)
        {
            return null;
        }

        var plan = contrato.ContractPlans?
            .Select(x => x.Plan)
            .FirstOrDefault(x => x is not null) ?? model.Plan;

        var lineaPlan = model.PrePaymentDetails?
            .FirstOrDefault(x => x.ServiceRequestDetailId is null);

        return new BillingContractDto
        {
            ContractClientId = model.ContractClientId,
            ClientId = model.ClientId,
            ControlContrato = contrato.ControlContrato,
            ClientFullName = $"{model.Client?.FirstName} {model.Client?.LastName}".Trim(),
            PhoneNumber = contrato.PhoneNumber,
            Address = contrato.Address,
            CityName = contrato.Zone?.City?.Name,
            ZoneName = contrato.Zone?.ZoneName,
            PlanId = plan?.PlanId ?? model.PlanId,
            PlanName = plan?.PlanName ?? model.Plan?.PlanName,
            TaxRate = lineaPlan?.TaxRate ?? model.TaxRate,
            PlanPrice = lineaPlan?.UnitPrice ?? model.UnitPrice,
            PlanPriceWithTax = lineaPlan?.PriceWithTax ?? model.PriceWithTax
        };
    }

    // Solo contratos ACTIVOS. Admite varias busquedas a la vez para no perder teclas, y
    // espera medio segundo antes de consultar.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchContractsAsync(string? texto)
    {
        SearchText = texto;

        _cts?.Cancel();

        if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length < MinimoParaBuscar)
        {
            Candidates = new ObservableCollection<BillingContractDto>();
            IsSearching = false;
            NoResults = false;
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

        IsSearching = true;
        NoResults = false;

        var response = await _repository.GetAsync<List<BillingContractDto>>(
            $"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(texto.Trim())}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        IsSearching = false;

        if (await _responseHandler.HandleErrorAsync(response))
        {
            Candidates = new ObservableCollection<BillingContractDto>();
            return;
        }

        Candidates = new ObservableCollection<BillingContractDto>(
            response.Response ?? new List<BillingContractDto>());

        NoResults = Candidates.Count == 0;
    }

    // Al elegir el contrato se pierden los servicios marcados: son de otro contrato
    [RelayCommand]
    private async Task SelectContractAsync(BillingContractDto? contrato)
    {
        if (contrato is null)
        {
            return;
        }

        _cts?.Cancel();
        IsSearching = false;
        NoResults = false;

        SelectedContract = contrato;
        SearchText = contrato.ClientFullName;
        ContractClientId = contrato.ContractClientId;

        _reservados.Clear();

        //La lista flotante se cierra sola al quedarse sin resultados
        Candidates = new ObservableCollection<BillingContractDto>();

        await LoadServicesAsync(contrato.ContractClientId);
    }

    // Solicitudes completadas, sin facturar y libres. Al editar se manda el Id del pago o
    // sus propios servicios no volverian y se perderian al guardar.
    private async Task LoadServicesAsync(Guid contractClientId)
    {
        ServicesLoading = true;
        Services = new ObservableCollection<PrePaymentServiceRow>();

        try
        {
            var url = $"{BaseUrl}/services/{contractClientId}";

            if (_prePaymentId != Guid.Empty)
            {
                url += $"?prePaymentId={_prePaymentId}";
            }

            var response = await _repository.GetAsync<List<PrePaymentServiceDto>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var filas = (response.Response ?? new List<PrePaymentServiceDto>())
                .Select(x => new PrePaymentServiceRow(x, RecalcularServicios)
                {
                    IsSelected = _reservados.Contains(x.ServiceRequestDetailId)
                });

            Services = new ObservableCollection<PrePaymentServiceRow>(filas);
        }
        finally
        {
            ServicesLoading = false;
            RecalcularServicios();
        }
    }

    private void RecalcularServicios()
    {
        ServicesTotal = Services.Where(x => x.IsSelected).Sum(x => x.Total);
    }

    // Del formulario solo viajan el contrato, el periodo, la fecha y los ids de servicio:
    // todo el dinero lo rearma el backend con los precios frescos de la base.
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (ContractClientId == Guid.Empty)
        {
            await _alertService.WarningAsync(
                "Hubo un problema con los datos enviados",
                "Debe seleccionar un contrato activo.");
            return;
        }

        if (DatePayment is null)
        {
            await _alertService.WarningAsync(
                "Hubo un problema con los datos enviados",
                "Debe indicar la fecha del pago.");
            return;
        }

        if (YearNumber <= 0)
        {
            await _alertService.WarningAsync(
                "Hubo un problema con los datos enviados",
                "Debe indicar el ano del periodo.");
            return;
        }

        IsSaving = true;

        try
        {
            var modelo = new PrePaymentEntity
            {
                PrePaymentId = _prePaymentId,
                DatePayment = DatePayment.Value,
                PaymentControl = _paymentControl,
                ContractClientId = ContractClientId,
                YearNumber = YearNumber,
                MonthType = _monthType,
                PrePaymentDetails = Services
                    .Where(x => x.IsSelected)
                    .Select(x => new PrePaymentDetail
                    {
                        ServiceRequestDetailId = x.ServiceRequestDetailId,
                        Concept = string.Empty
                    })
                    .ToList()
            };

            var response = IsEdit
                ? await _repository.PutAsync(BaseUrl, modelo)
                : await _repository.PostAsync(BaseUrl, modelo);

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

// Un servicio que se puede adelantar, con su casilla
public partial class PrePaymentServiceRow : ObservableObject
{
    private readonly Action _alMarcar;

    public PrePaymentServiceDto Item { get; }

    [ObservableProperty]
    private bool _isSelected;

    public Guid ServiceRequestDetailId => Item.ServiceRequestDetailId;

    public string Title => $"#{Item.RequestNumber} · {Item.ServiceName}";

    public string Meta => string.Join(" · ", new[]
    {
        Item.CompletedAtUtc?.ToLocalTime().ToString("dd/MM/yyyy"),
        Item.Detail
    }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public decimal Total => Item.Total;

    public PrePaymentServiceRow(PrePaymentServiceDto item, Action alMarcar)
    {
        Item = item;
        _alMarcar = alMarcar;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        _alMarcar();
    }
}
