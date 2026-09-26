using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesBilling.BillingNoteOne;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Globalization;
using NoteEntity = Spix.Domain.EntitiesBilling.BillingNoteOne;

namespace Spix.AppWpf.ViewModels.EntitiesBilling.BillingNoteOne;

// Nota de cobro individual: la nota de UN solo cliente, el que queda fuera del lanzamiento
// general porque se instalo despues del corte o porque hay que cobrarle un periodo aparte.
//
// El ciclo tiene dos tiempos. Primero se CREA la nota (fecha, contrato activo, ano y mes) y
// queda Pendiente sin mover un peso. Despues se abre el detalle, el sistema muestra que se
// le va a cobrar y que le falta al contrato, y solo si no hay ningun impedimento aparece el
// boton de lanzar. Al lanzar nacen la factura y la cuenta por cobrar, y la nota ya no se
// puede editar ni borrar.
//
// No toca el MikroTik y no genera PDF: solo escribe en la base.
public partial class BillingNoteOneIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnoteones";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    //El nombre del mes lo arma el BACKEND en el combo; aqui solo se busca por su numero
    private readonly Dictionary<int, string> _months = new();

    [ObservableProperty]
    private ObservableCollection<BillingNoteOneRow> _rows = new();

    [ObservableProperty]
    private string? _filter;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== El tablero del ano =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _summaryYear;

    [ObservableProperty]
    private int _summaryNotes;

    [ObservableProperty]
    private int _summaryLaunched;

    [ObservableProperty]
    private int _summaryPending;

    [ObservableProperty]
    private decimal _summaryBilled;

    public string NotesLabel => $"Notas de {SummaryYear}";

    public string BilledLabel => $"Facturado en {SummaryYear}";

    public BillingNoteOneIndexViewModel(
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

    partial void OnSummaryYearChanged(int value)
    {
        OnPropertyChanged(nameof(NotesLabel));
        OnPropertyChanged(nameof(BilledLabel));
    }

    // El orden importa: sin el combo de meses la columna Periodo saldria en ingles
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

        _months.Clear();

        foreach (var item in response.Response ?? new List<IntItemModel>())
        {
            _months[item.Value] = item.Name ?? string.Empty;
        }
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<BillingNoteSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new BillingNoteSummaryDto();

        SummaryYear = datos.YearNumber;
        SummaryNotes = datos.Notes;
        SummaryLaunched = datos.Launched;
        SummaryPending = datos.Pending;
        SummaryBilled = datos.Billed;
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

            var response = await _repository.GetAsync<List<NoteEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<BillingNoteOneRow>(
                (response.Response ?? new List<NoteEntity>())
                    .Select(x => new BillingNoteOneRow(x, MonthName(x.MonthType))));

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
        return _months.TryGetValue((int)monthType, out var nombre) && !string.IsNullOrWhiteSpace(nombre)
            ? nombre
            : monthType.ToString();
    }

    // Despues de crear, editar, borrar o lanzar: primero el tablero y despues la lista,
    // igual que en la web
    private async Task ReloadAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await ReloadAsync();
    }

    //Buscar vuelve a la primera pagina: en la web la marca se queda en la pagina vieja y eso
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

    [RelayCommand]
    private async Task NewAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["Id"] = Guid.Empty
        };

        var result = await _modalService.ShowAsync<BillingNoteOneFormDialogView>(
            "Nueva Nota Individual", parametros);

        if (result.Succeeded)
        {
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync(BillingNoteOneRow? fila)
    {
        if (fila is null || fila.Created)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.BillingNoteOneId
        };

        var result = await _modalService.ShowAsync<BillingNoteOneFormDialogView>(
            "Editar Nota Cobro", parametros);

        if (result.Succeeded)
        {
            await ReloadAsync();
        }
    }

    // El detalle esta en TODAS las filas, lanzadas y pendientes: es donde se lanza la nota
    [RelayCommand]
    private async Task DetailAsync(BillingNoteOneRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.BillingNoteOneId
        };

        var result = await _modalService.ShowAsync<BillingNoteOneDetailDialogView>(
            "Detalle Nota Cobro Cliente", parametros);

        if (result.Succeeded)
        {
            await ReloadAsync();
        }
    }

    // Borrado FISICO, y solo mientras la nota siga pendiente
    [RelayCommand]
    private async Task DeleteAsync(BillingNoteOneRow? fila)
    {
        if (fila is null || fila.Created)
        {
            return;
        }

        //Los textos de confirmacion y de exito van CRUZADOS a proposito: son los mismos que
        //usa la web, y si se "corrigen" aqui las dos pantallas dejan de verse iguales
        var confirmado = await _alertService.ConfirmAsync(
            "Borrado",
            "El registro borrado con exito",
            "Si, Borralo");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.BillingNoteOneId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync(
                "Confirmar Borrado",
                "Esta seguro de borrar este registro?");
        }
        finally
        {
            IsLoading = false;
        }

        await ReloadAsync();
    }
}

// Una fila de la tabla, ya lista para pintar
public class BillingNoteOneRow
{
    public NoteEntity Item { get; }

    public Guid BillingNoteOneId => Item.BillingNoteOneId;

    public string ClientName => $"{Item.Client?.FirstName} {Item.Client?.LastName}".Trim();

    public string MetaText => $"Contrato #{Item.ContractClient?.ControlContrato} · {Item.DateBill:dd/MM/yyyy}";

    public string PeriodText { get; }

    public bool Created => Item.Created;

    //Una nota lanzada ya no se edita ni se borra: el dinero ya se movio
    public bool IsPending => !Item.Created;

    public string CreatedDateText => Item.DateCreated?.ToString("dd/MM/yyyy") ?? string.Empty;

    public BillingNoteOneRow(NoteEntity item, string monthName)
    {
        Item = item;
        PeriodText = $"{monthName} {item.YearNumber}";
    }
}

// Crear o editar la nota: la fecha, el contrato ACTIVO y el periodo que se le va a cobrar.
//
// Es un solo formulario para los dos casos porque lo unico que cambia es de donde salen los
// datos al abrir y a que verbo se manda al guardar; los campos, los combos y las reglas son
// exactamente los mismos.
//
// Aqui no se mueve un peso: crear la nota solo la deja Pendiente.
public partial class BillingNoteOneFormDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnoteones";

    //Minimo de letras y pausa antes de consultar. Son 2 y no 3 porque el backend tambien
    //corta en 2: con 3 el WPF dejaria fuera busquedas que la web si resuelve.
    private const int MinimoParaBuscar = 2;
    private const int PausaMs = 500;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    private CancellationTokenSource? _cts;
    private Guid _id;
    private int _ultimoMesValido = 1;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private DateTime _dateBill = DateTime.Today;

    [ObservableProperty]
    private int _yearNumber = DateTime.Today.Year;

    [ObservableProperty]
    private int _selectedMonth = DateTime.Today.Month;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private ObservableCollection<BillingContractDto> _candidates = new();

    [ObservableProperty]
    private BillingContractDto? _selected;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private bool _noResults;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    //El estado de la nota solo se muestra en la edicion, y nunca se puede tocar
    [ObservableProperty]
    private bool _showCreated;

    [ObservableProperty]
    private bool _created;

    [ObservableProperty]
    private string? _createdDateText;

    public bool HasSelected => Selected is not null;

    public string SelectedText => Selected is null
        ? string.Empty
        : $"#{Selected.ControlContrato} · {Selected.ClientFullName}";

    public string ContractText => Selected is null
        ? string.Empty
        : Selected.ControlContrato.ToString(CultureInfo.CurrentCulture);

    public string? AddressText => Selected?.Address;

    public string? CityText => Selected?.CityName;

    public string? ZoneText => Selected?.ZoneName;

    public string? PhoneText => Selected?.PhoneNumber;

    public string? PlanText => Selected?.PlanName;

    public string PriceText => Selected?.PlanPrice?.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;

    public BillingNoteOneFormDialogViewModel(
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

    partial void OnSelectedChanged(BillingContractDto? value)
    {
        OnPropertyChanged(nameof(HasSelected));
        OnPropertyChanged(nameof(SelectedText));
        OnPropertyChanged(nameof(ContractText));
        OnPropertyChanged(nameof(AddressText));
        OnPropertyChanged(nameof(CityText));
        OnPropertyChanged(nameof(ZoneText));
        OnPropertyChanged(nameof(PhoneText));
        OnPropertyChanged(nameof(PlanText));
        OnPropertyChanged(nameof(PriceText));
    }

    // La fecha reescribe de golpe el ano y el mes. Al reves NO: cambiar el ano o el mes por
    // separado no toca la fecha, porque la fecha es cuando se emite y el periodo es lo que
    // se cobra, y pueden ser distintos.
    partial void OnDateBillChanged(DateTime value)
    {
        YearNumber = value.Year;
        SelectedMonth = value.Month;
    }

    // El neutro [Seleccione Mes] del combo vale 0, que no es un mes: se descarta y queda el
    // anterior, sin ningun aviso, igual que en la web
    partial void OnSelectedMonthChanged(int value)
    {
        if (!Enum.IsDefined(typeof(MonthType), value))
        {
            SelectedMonth = _ultimoMesValido;
            return;
        }

        _ultimoMesValido = value;
    }

    public async Task InitializeAsync(Guid id)
    {
        _id = id;
        ShowCreated = id != Guid.Empty;

        IsLoading = true;

        try
        {
            await LoadMonthsAsync();

            if (id == Guid.Empty)
            {
                //Nota nueva: la fecha de hoy manda, y de ella salen el ano y el mes
                DateBill = DateTime.Today;
                return;
            }

            var response = await _repository.GetAsync<NoteEntity>($"{BaseUrl}/{id}");
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

            //La fecha va PRIMERO: al asignarla reescribe el ano y el mes, asi que el periodo
            //guardado tiene que ponerse despues o se perderia
            DateBill = model.DateBill;
            YearNumber = model.YearNumber;
            SelectedMonth = (int)model.MonthType;

            Created = model.Created;
            CreatedDateText = model.DateCreated?.ToString("dd/MM/yyyy");

            Selected = BuildContract(model);
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

        //La lista llega ARMADA del backend, con el [Seleccione Mes] en la posicion 0
        Months = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    // El GET por id trae el contrato con su zona, su ciudad y su plan: de ahi se arma la
    // misma ficha que devuelve la busqueda
    private static BillingContractDto? BuildContract(NoteEntity model)
    {
        var contract = model.ContractClient;

        if (contract is null)
        {
            return null;
        }

        var plan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null)?.Plan;

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
            PlanName = plan?.PlanName,
            PlanPrice = plan?.Price
        };
    }

    // Solo se ofrecen contratos ACTIVOS. Admite varias busquedas a la vez para no perder
    // teclas, y espera medio segundo antes de consultar.
    //
    // A diferencia del molde, escribir NO borra el contrato ya elegido: en la edicion se
    // llega con uno puesto y una tecla suelta lo dejaria sin contrato. La ficha de arriba es
    // la que manda: solo cambia al elegir de la lista.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task SearchAsync(string? texto)
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

    [RelayCommand]
    private void Select(BillingContractDto? item)
    {
        if (item is null)
        {
            return;
        }

        Selected = item;

        _cts?.Cancel();
        Candidates = new ObservableCollection<BillingContractDto>();
        IsSearching = false;
        NoResults = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Selected is null)
        {
            await _alertService.WarningAsync("Validacion", "Debe seleccionar un contrato activo.");
            return;
        }

        IsSaving = true;

        try
        {
            //Solo viajan los campos que el servicio lee; el ClientId lo pisa el backend con
            //el del contrato, y el estado, la corporacion y el usuario los pone el servidor
            var model = new NoteEntity
            {
                BillingNoteOneId = _id,
                DateBill = DateBill,
                ClientId = Selected.ClientId,
                ContractClientId = Selected.ContractClientId,
                YearNumber = YearNumber,
                MonthType = (MonthType)SelectedMonth
            };

            if (_id == Guid.Empty)
            {
                var creada = await _repository.PostAsync(BaseUrl, model);
                if (await _responseHandler.HandleErrorAsync(creada))
                {
                    return;
                }

                await _alertService.SuccessAsync("Registrado", "El Registro se realizo con exito");
                await _modalService.CloseAsync(ModalResult.Ok());
                return;
            }

            //La edicion no avisa nada: cierra y el indice recarga, igual que en la web
            var editada = await _repository.PutAsync(BaseUrl, model);
            if (await _responseHandler.HandleErrorAsync(editada))
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

// El detalle de la nota y el sitio donde se LANZA.
//
// Mientras la nota esta pendiente se consulta al servidor que se le va a cobrar y que le
// falta al contrato. Si falta algo se pintan las etiquetas rojas y el boton de lanzar ni
// siquiera aparece.
//
// Una nota ya lanzada no consulta nada: solo se ve la ficha en solo lectura.
public partial class BillingNoteOneDetailDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnoteones";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    private Guid _id;
    private string _checkClientName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLaunching;

    //===== La ficha de la nota =====
    [ObservableProperty]
    private string? _clientName;

    [ObservableProperty]
    private string? _contractText;

    [ObservableProperty]
    private string? _addressText;

    [ObservableProperty]
    private string? _placeText;

    [ObservableProperty]
    private string? _phoneText;

    [ObservableProperty]
    private string? _planText;

    [ObservableProperty]
    private string? _priceText;

    [ObservableProperty]
    private string? _dateBillText;

    [ObservableProperty]
    private string? _periodText;

    [ObservableProperty]
    private bool _created;

    [ObservableProperty]
    private string? _createdDateText;

    //===== Lo que se le va a cobrar =====
    [ObservableProperty]
    private bool _hasCheck;

    [ObservableProperty]
    private ObservableCollection<BillingOneLineRow> _lines = new();

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _prePayment;

    [ObservableProperty]
    private decimal _exonerated;

    [ObservableProperty]
    private decimal _balance;

    [ObservableProperty]
    private ObservableCollection<string> _blocks = new();

    [ObservableProperty]
    private bool _hasBlocks;

    //Las tres condiciones juntas: nota pendiente, revision recibida y nada que la bloquee
    [ObservableProperty]
    private bool _canLaunch;

    public BillingNoteOneDetailDialogViewModel(
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

    public async Task InitializeAsync(Guid id)
    {
        _id = id;

        IsLoading = true;

        try
        {
            var meses = await LoadMonthsAsync();

            var response = await _repository.GetAsync<NoteEntity>($"{BaseUrl}/{id}");
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

            Fill(model, meses);

            if (!model.Created)
            {
                await LoadCheckAsync(id);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<Dictionary<int, string>> LoadMonthsAsync()
    {
        var meses = new Dictionary<int, string>();

        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return meses;
        }

        foreach (var item in response.Response ?? new List<IntItemModel>())
        {
            meses[item.Value] = item.Name ?? string.Empty;
        }

        return meses;
    }

    private void Fill(NoteEntity model, Dictionary<int, string> meses)
    {
        var contract = model.ContractClient;
        var plan = contract?.ContractPlans?.FirstOrDefault(x => x.Plan != null)?.Plan;

        ClientName = $"{model.Client?.FirstName} {model.Client?.LastName}".Trim();
        ContractText = $"Contrato #{contract?.ControlContrato}";
        AddressText = contract?.Address;
        PlaceText = string.Join(" · ",
            new[] { contract?.Zone?.City?.Name, contract?.Zone?.ZoneName }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        PhoneText = contract?.PhoneNumber;
        PlanText = plan?.PlanName;
        PriceText = plan?.Price.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;
        DateBillText = model.DateBill.ToString("dd/MM/yyyy");

        var mes = meses.TryGetValue((int)model.MonthType, out var nombre) && !string.IsNullOrWhiteSpace(nombre)
            ? nombre
            : model.MonthType.ToString();

        PeriodText = $"{mes} {model.YearNumber}";

        Created = model.Created;
        CreatedDateText = model.DateCreated?.ToString("dd/MM/yyyy") ?? string.Empty;
    }

    private async Task LoadCheckAsync(Guid id)
    {
        var response = await _repository.GetAsync<BillingOneCheckDto>($"{BaseUrl}/{id}/check");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var check = response.Response;

        if (check is null)
        {
            return;
        }

        _checkClientName = check.ClientFullName;

        var lineas = new List<BillingOneLineRow>();

        if (!string.IsNullOrWhiteSpace(check.PlanName))
        {
            lineas.Add(new BillingOneLineRow(check.PlanName!, "Plan", check.PlanPrice));
        }

        foreach (var item in check.Services)
        {
            lineas.Add(new BillingOneLineRow(item.Concept, "Servicio tecnico", item.Price));
        }

        Lines = new ObservableCollection<BillingOneLineRow>(lineas);

        Total = check.Total;
        PrePayment = check.PrePayment;
        Exonerated = check.Exonerated;

        //El saldo puede quedar NEGATIVO si el adelanto o la exoneracion superan el total, y
        //se muestra tal cual: no se recorta a cero
        Balance = check.Balance;

        Blocks = new ObservableCollection<string>(BuildBlocks(check));
        HasBlocks = Blocks.Count > 0;
        HasCheck = true;
        CanLaunch = !Created && HasCheck && Blocks.Count == 0;
    }

    // Lo que impide lanzar la nota. El orden es el mismo de la web, y las siete etiquetas de
    // lo que le falta al contrato van en espanol sin traducir, igual que en el backend.
    private static List<string> BuildBlocks(BillingOneCheckDto check)
    {
        var faltas = new List<string>();

        if (!check.IsActive)
        {
            faltas.Add("El contrato no esta activo.");
        }

        if (check.AlreadyBilled)
        {
            faltas.Add($"El contrato {check.ControlContrato} ya tiene una factura o una cuenta por cobrar para el periodo seleccionado.");
        }

        if (check.PrePaymentAndExonerated)
        {
            faltas.Add("El mismo mes tiene exoneracion y pago adelantado.");
        }

        if (!check.HasPlan)
        {
            faltas.Add("Plan");
        }

        if (!check.HasIp)
        {
            faltas.Add("IP");
        }

        if (!check.HasMac)
        {
            faltas.Add("MAC");
        }

        if (!check.HasServer)
        {
            faltas.Add("Servidor");
        }

        if (!check.HasNode)
        {
            faltas.Add("Nodo");
        }

        if (!check.HasQueue)
        {
            faltas.Add("Queue");
        }

        if (!check.HasBinding)
        {
            faltas.Add("IpBinding");
        }

        return faltas;
    }

    // Aqui se mueve el dinero: nacen la factura y la cuenta por cobrar, se cruzan el pago
    // adelantado y la exoneracion del mes, y la nota queda cerrada para siempre.
    [RelayCommand]
    private async Task LaunchAsync()
    {
        if (!HasCheck || IsLaunching)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Nota de Cobro Individual",
            $"Se va a generar la nota de cobro de {_checkClientName} con un saldo de {Balance.ToString("N2", CultureInfo.CurrentCulture)}.",
            "Nota de Cobro Individual");

        if (!confirmado)
        {
            return;
        }

        IsLaunching = true;

        try
        {
            //El id va en la ruta; el cuerpo lo ignora el controlador
            var response = await _repository.PostAsync($"{BaseUrl}/{_id}/launch", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Nota de Cobro Individual", "Nota generada correctamente.");
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsLaunching = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Un renglon de lo que se le va a cobrar
public class BillingOneLineRow
{
    public string Concept { get; }

    public string OriginName { get; }

    public decimal Price { get; }

    public BillingOneLineRow(string concept, string originName, decimal price)
    {
        Concept = concept;
        OriginName = originName;
        Price = price;
    }
}
