using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ExoneratedEntity = Spix.Domain.EntitiesPayment.ContractExonerated;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.ContractExonerated;

// Exoneracion del mes: se programa que un contrato NO pague un ano/mes concreto.
//
// Esta pantalla no mueve plata. Guarda la exoneracion con el monto congelado del plan y
// quien la consume es la facturacion: al lanzar la nota de cobro de ese contrato para ese
// periodo, el descuento entra en el detalle y la exoneracion queda marcada como aplicada.
//
// Es la hermana de la exoneracion FIJA (ContractExempt), que no tiene vencimiento.
//
// No habla con el MikroTik y no genera PDF ni recibos.
public partial class ContractExoneratedIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractexonerateds";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    //Los nombres de los meses llegan traducidos del backend; aqui solo se buscan por valor
    private List<IntItemModel> _months = new();

    [ObservableProperty]
    private ObservableCollection<ExoneratedRow> _rows = new();

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

    public ContractExoneratedIndexViewModel(
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

    // Los meses primero: sin ellos el periodo de cada fila saldria con el nombre del enum
    // en ingles
    public async Task InitializeAsync()
    {
        await LoadMonthsAsync();
        await LoadSummaryAsync();
        await LoadAsync(1);
    }

    private async Task LoadMonthsAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        _months = response.Response ?? new List<IntItemModel>();
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ExoneratedSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new ExoneratedSummaryDto();

        Pending = datos.Pending;
        PendingTotal = datos.PendingTotal;
        Contracts = datos.Contracts;

        //El cuarto indicador es DINERO aplicado en el mes, no el conteo: la web tambien
        //recibe BilledMonth y tampoco lo pinta
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

            var response = await _repository.GetAsync<List<ExoneratedEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ExoneratedRow>(
                (response.Response ?? new List<ExoneratedEntity>())
                    .Select(x => new ExoneratedRow(x, MonthName(x.MonthType))));

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

    private string MonthName(MonthType monthType)
    {
        return _months.FirstOrDefault(x => x.Value == (int)monthType)?.Name ?? monthType.ToString();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
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

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateContractExoneratedDialogView>("Nueva Exoneracion");

        //El tablero solo se recarga si se guardo; la lista se recarga siempre, igual que
        //en la web. El aviso de exito lo da el propio modal antes de cerrarse.
        if (result.Succeeded)
        {
            await LoadSummaryAsync();
        }

        await LoadAsync(CurrentPage);
    }

    [RelayCommand]
    private async Task EditAsync(ExoneratedRow? item)
    {
        if (item is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = item.ContractExoneratedId
        };

        var result = await _modalService.ShowAsync<EditContractExoneratedDialogView>(
            "Editar Exoneracion", parametros);

        if (result.Succeeded)
        {
            await LoadSummaryAsync();
        }

        await LoadAsync(CurrentPage);
    }

    // Eliminar es un CIERRE: la fila se queda en la base con su fecha de cierre y su
    // usuario, y desaparece tanto de la lista como del tablero.
    [RelayCommand]
    private async Task DeleteAsync(ExoneratedRow? item)
    {
        if (item is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar",
            "Desea eliminar esta exoneracion?",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.DeleteAsync($"{BaseUrl}/{item.ContractExoneratedId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Eliminado", "Registro eliminado correctamente.");
        }
        finally
        {
            IsLoading = false;
        }

        await LoadSummaryAsync();
        await LoadAsync(CurrentPage);
    }

    // No consulta nada: la auditoria sale de lo que ya trajo la lista
    [RelayCommand]
    private async Task AuditAsync(ExoneratedRow? item)
    {
        if (item is null)
        {
            return;
        }

        //Las etiquetas vienen recicladas del modulo de suspendidos; se copian tal cual
        //para que la pantalla lea igual que la web
        var pares = new (string Etiqueta, string? Valor)[]
        {
            ("Registrado por", item.UserByName),
            ("Fecha de suspension", item.DateExoneratedText),
            ("Periodo exonerado", item.PeriodText),
            ("Motivo", item.Motivo),
            ("Facturado el", item.DateBilledText),
            ("Reactivado por", item.UserByNameEnded),
            ("Fecha de reactivacion", item.DateEndedText)
        };

        var lineas = pares
            .Where(x => !string.IsNullOrWhiteSpace(x.Valor))
            .Select(x => $"{x.Etiqueta}: {x.Valor}")
            .ToList();

        if (lineas.Count == 0)
        {
            lineas.Add("-");
        }

        await _alertService.WarningAsync(
            "Auditoria del registro",
            string.Join(Environment.NewLine, lineas));
    }
}

// Una fila de la tabla, ya lista para pintar
public class ExoneratedRow
{
    public ExoneratedEntity Item { get; }

    //El nombre del mes lo resuelve el index con el combo del backend
    public string PeriodText { get; }

    public Guid ContractExoneratedId => Item.ContractExoneratedId;

    public string ClientFullName => $"{Item.Client?.FirstName} {Item.Client?.LastName}".Trim();

    public string ContractText => $"Contrato #{Item.ContractClient?.ControlContrato}";

    public string DateExoneratedText => Item.DateExonerated.ToString("dd/MM/yyyy");

    public string? PlanName => Item.Plan?.PlanName;

    public decimal Amount => Item.PriceWithTax;

    public bool Billed => Item.Billed;

    // Editar y Borrar solo mientras no este aplicada. La consulta del backend ya descarta
    // las facturadas, asi que en la practica siempre se pueden usar; la condicion se
    // mantiene porque es la de la web.
    public bool CanManage => !Item.Billed;

    public string? UserByName => Item.UserByName;

    public string? Motivo => Item.Motivo;

    public string? DateBilledText => Item.DateBilled?.ToString("dd/MM/yyyy");

    public string? UserByNameEnded => Item.UserByNameEnded;

    //La unica fecha del modulo que se guarda con hora, y la unica que se pasa a hora local
    public string? DateEndedText => Item.DateEnded?.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public ExoneratedRow(ExoneratedEntity item, string periodText)
    {
        Item = item;
        PeriodText = periodText;
    }
}

// Lo que comparten el alta y la edicion: buscar el contrato y elegir el periodo.
//
// Los montos que se ven son solo para mirar: el servidor relee el plan del contrato y
// recalcula TaxRate, UnitPrice y PriceWithTax, mande el escritorio lo que mande.
public abstract partial class ContractExoneratedFormViewModel : ObservableObject
{
    protected const string BaseUrl = "api/v1/contractexonerateds";

    //Minimo de letras y pausa antes de consultar, igual que el autocompletar de la web
    private const int MinimoParaBuscar = 2;
    private const int PausaMs = 500;

    protected readonly IRepository Repository;
    protected readonly HttpResponseHandler ResponseHandler;
    protected readonly ModalService ModalService;
    protected readonly AlertService AlertService;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private DateTime? _dateExonerated;

    [ObservableProperty]
    private int _yearNumber;

    //El valor del combo, no el enum: el neutro del backend vale 0 y MonthType no define 0
    [ObservableProperty]
    private int _selectedMonth;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<BillingContractDto> _candidates = new();

    [ObservableProperty]
    private BillingContractDto? _selectedContract;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _noResults;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    //El bloque de facturado solo existe en la edicion
    [ObservableProperty]
    private bool _showBilled;

    [ObservableProperty]
    private bool _billed;

    [ObservableProperty]
    private DateTime? _dateBilled;

    private CancellationTokenSource? _cts;

    //Mientras se arma la pantalla desde el registro guardado, escribir el nombre del
    //cliente en el buscador no debe disparar una consulta
    protected bool CargandoModelo;

    public bool HasContract => SelectedContract is not null;

    public string DateBilledText => DateBilled?.ToString("dd/MM/yyyy") ?? string.Empty;

    protected ContractExoneratedFormViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        Repository = repository;
        ResponseHandler = responseHandler;
        ModalService = modalService;
        AlertService = alertService;
    }

    partial void OnSelectedContractChanged(BillingContractDto? value) => OnPropertyChanged(nameof(HasContract));

    partial void OnDateBilledChanged(DateTime? value) => OnPropertyChanged(nameof(DateBilledText));

    protected async Task LoadMonthsAsync()
    {
        var response = await Repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await ResponseHandler.HandleErrorAsync(response))
        {
            return;
        }

        //La lista llega armada del backend, con su neutro en la posicion 0: aqui no se
        //filtra, no se ordena y no se agrega nada
        Months = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    // Solo se ofrecen contratos ACTIVOS. Admite varias busquedas a la vez para no perder
    // teclas, y espera medio segundo antes de consultar.
    //
    // A diferencia de la exoneracion fija, escribir NO suelta el contrato ya elegido: la
    // edicion abre con el nombre del cliente puesto y perderia los montos.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchAsync(string? texto)
    {
        if (CargandoModelo)
        {
            return;
        }

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

        var response = await Repository.GetAsync<List<BillingContractDto>>(
            $"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(texto.Trim())}");

        if (token.IsCancellationRequested)
        {
            return;
        }

        IsSearching = false;

        if (await ResponseHandler.HandleErrorAsync(response))
        {
            Candidates = new ObservableCollection<BillingContractDto>();
            return;
        }

        Candidates = new ObservableCollection<BillingContractDto>(
            response.Response ?? new List<BillingContractDto>());

        NoResults = Candidates.Count == 0;
    }

    [RelayCommand]
    private void Select(BillingContractDto? item)
    {
        if (item is null)
        {
            return;
        }

        SelectedContract = item;
        SearchText = item.ClientFullName;
        Candidates = new ObservableCollection<BillingContractDto>();

        _cts?.Cancel();
        IsSearching = false;
        NoResults = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedContract is null || SelectedContract.ContractClientId == Guid.Empty)
        {
            await AlertService.WarningAsync("Validacion", "Debe seleccionar un contrato activo.");
            return;
        }

        //El neutro del combo vale 0: al crear el backend lo corrige solo, pero al editar
        //lo guardaria tal cual, asi que aqui no pasa
        if (!Enum.IsDefined(typeof(MonthType), SelectedMonth))
        {
            await AlertService.WarningAsync("Validacion", "Debe seleccionar el mes.");
            return;
        }

        IsSaving = true;

        try
        {
            await SubmitAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await ModalService.CloseAsync(ModalResult.Cancel());
    }

    protected abstract Task SubmitAsync();

    // Se manda el objeto plano: el servicio solo lee el periodo, la fecha y el contrato, y
    // recalcula el dinero desde el plan. Mandar las navegaciones del GET no aporta nada.
    protected ExoneratedEntity BuildPayload()
    {
        return new ExoneratedEntity
        {
            DateExonerated = (DateExonerated ?? DateTime.UtcNow).Date,
            ContractClientId = SelectedContract!.ContractClientId,
            ClientId = SelectedContract.ClientId,
            PlanId = SelectedContract.PlanId,
            TaxRate = SelectedContract.TaxRate ?? 0,
            UnitPrice = SelectedContract.PlanPrice ?? 0,
            PriceWithTax = SelectedContract.PlanPriceWithTax ?? 0,
            YearNumber = YearNumber,
            MonthType = (MonthType)SelectedMonth
        };
    }
}

// Nueva exoneracion: arranca apuntando al MES SIGUIENTE, con la fecha de hoy.
public partial class CreateContractExoneratedDialogViewModel : ContractExoneratedFormViewModel
{
    public CreateContractExoneratedDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            //Primero los meses: el combo no puede resolver el mes elegido con la lista vacia
            await LoadMonthsAsync();

            var siguiente = DateTime.UtcNow.AddMonths(1);

            DateExonerated = DateTime.UtcNow.Date;
            YearNumber = siguiente.Year;
            SelectedMonth = siguiente.Month;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected override async Task SubmitAsync()
    {
        var response = await Repository.PostAsync(BaseUrl, BuildPayload());
        if (await ResponseHandler.HandleErrorAsync(response))
        {
            return;
        }

        //El aviso lo da el alta antes de cerrarse, igual que en la web
        await AlertService.SuccessAsync("Registrado", "El Registro se realizo con exito");
        await ModalService.CloseAsync(ModalResult.Ok());
    }
}

// Editar una exoneracion programada.
//
// Guardar la RE-COTIZA: el servicio vuelve a leer el plan, asi que si el precio cambio
// desde que se programo, el monto exonerado queda al precio de hoy.
public partial class EditContractExoneratedDialogViewModel : ContractExoneratedFormViewModel
{
    private Guid _id;

    public EditContractExoneratedDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
    }

    public async Task InitializeAsync(Guid id)
    {
        _id = id;

        IsLoading = true;
        CargandoModelo = true;

        try
        {
            await LoadMonthsAsync();
            await LoadModelAsync();
        }
        finally
        {
            CargandoModelo = false;
            IsLoading = false;
        }
    }

    private async Task LoadModelAsync()
    {
        var response = await Repository.GetAsync<ExoneratedEntity>($"{BaseUrl}/{_id}");
        if (await ResponseHandler.HandleErrorAsync(response))
        {
            await ModalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        var model = response.Response;
        if (model is null)
        {
            await ModalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        DateExonerated = model.DateExonerated;
        YearNumber = model.YearNumber;
        SelectedMonth = (int)model.MonthType;

        ShowBilled = true;
        Billed = model.Billed;
        DateBilled = model.DateBilled;

        SelectedContract = BuildContractDto(model);
        SearchText = SelectedContract?.ClientFullName;
    }

    // El contrato se rearma desde el registro GUARDADO: los montos son los que se
    // congelaron el dia de la exoneracion, no los del plan de hoy.
    private static BillingContractDto? BuildContractDto(ExoneratedEntity model)
    {
        var contract = model.ContractClient;
        if (contract is null)
        {
            return null;
        }

        var plan = contract.ContractPlans?.Select(x => x.Plan).FirstOrDefault() ?? model.Plan;

        return new BillingContractDto
        {
            ContractClientId = contract.ContractClientId,
            ClientId = contract.ClientId,
            ControlContrato = contract.ControlContrato,
            ClientFullName = $"{model.Client?.FirstName} {model.Client?.LastName}".Trim(),
            PhoneNumber = contract.PhoneNumber,
            Address = contract.Address,
            CityName = contract.Zone?.City?.Name,
            ZoneName = contract.Zone?.ZoneName,
            PlanId = plan?.PlanId ?? model.PlanId,
            PlanName = plan?.PlanName ?? model.Plan?.PlanName,
            PlanPrice = model.UnitPrice,
            TaxRate = model.TaxRate,
            PlanPriceWithTax = model.PriceWithTax
        };
    }

    protected override async Task SubmitAsync()
    {
        var payload = BuildPayload();
        payload.ContractExoneratedId = _id;

        var response = await Repository.PutAsync(BaseUrl, payload);
        if (await ResponseHandler.HandleErrorAsync(response))
        {
            return;
        }

        //La edicion no confirma nada: solo cierra. Es asi tambien en la web.
        await ModalService.CloseAsync(ModalResult.Ok());
    }
}
