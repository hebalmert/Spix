using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesBilling.BillingNote;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using BillingNoteEntity = Spix.Domain.EntitiesBilling.BillingNote;

namespace Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;

// Notas de cobro generales: una nota por periodo (ano + mes) y, desde su detalle, el
// lanzamiento de la facturacion MASIVA del mes.
//
// Aqui nace el ciclo del dinero: al lanzar, cada contrato ACTIVO recibe su factura y su
// cuenta por cobrar. Una nota ya lanzada se congela: no se edita y no se borra, solo se
// mira su detalle.
//
// Este modulo NO habla con el MikroTik y NO genera PDF: solo escribe dinero en la base.
public partial class BillingNoteIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnotes";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    //Los meses traducidos: la fila los necesita para escribir el periodo
    private List<IntItemModel> _months = new();

    [ObservableProperty]
    private ObservableCollection<BillingNoteRow> _rows = new();

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
    private string _yearLabel = "Notas del ano";

    [ObservableProperty]
    private int _notes;

    [ObservableProperty]
    private int _launched;

    [ObservableProperty]
    private int _pending;

    [ObservableProperty]
    private int _activeContracts;

    public BillingNoteIndexViewModel(
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

    // Los meses PRIMERO: sin ellos el periodo de cada fila saldria con el nombre ingles
    // del enum
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
        var response = await _repository.GetAsync<BillingNoteSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new BillingNoteSummaryDto();

        YearLabel = $"Notas de {datos.YearNumber}";
        Notes = datos.Notes;
        Launched = datos.Launched;
        Pending = datos.Pending;
        ActiveContracts = datos.ActiveContracts;
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

            var response = await _repository.GetAsync<List<BillingNoteEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<BillingNoteRow>(
                (response.Response ?? new List<BillingNoteEntity>())
                    .Select(x => new BillingNoteRow(x, MonthName(x.MonthType))));

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
        var item = _months.FirstOrDefault(x => x.Value == (int)monthType);

        return string.IsNullOrWhiteSpace(item?.Name) ? monthType.ToString() : item.Name!;
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

    [RelayCommand]
    private async Task NewAsync()
    {
        var resultado = await _modalService.ShowAsync<CreateBillingNoteDialogView>("Nueva nota general");

        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
        await _alertService.SuccessAsync("Registrado", "El registro se realizo con exito.");
    }

    [RelayCommand]
    private async Task EditAsync(BillingNoteRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.BillingNoteId
        };

        var resultado = await _modalService.ShowAsync<EditBillingNoteDialogView>(
            "Editar nota general", parametros);

        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    // El detalle es la pantalla de lanzamiento: al volver hay que refrescar el tablero
    [RelayCommand]
    private async Task DetailAsync(BillingNoteRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.BillingNoteId
        };

        var resultado = await _modalService.ShowAsync<BillingNoteDetailDialogView>(
            $"Detalle de la nota {fila.PeriodText}", parametros);

        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    [RelayCommand]
    private async Task DeleteAsync(BillingNoteRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Borrar la nota general",
            $"Se va a borrar la nota de {fila.PeriodText}. Desea continuar?",
            "Si, borrarla");

        if (!confirmado)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.BillingNoteId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await LoadSummaryAsync();
            await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
            await _alertService.SuccessAsync("Borrado", "El registro se borro con exito.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

// Una fila de la tabla, ya lista para pintar
public class BillingNoteRow
{
    public BillingNoteEntity Item { get; }

    public Guid BillingNoteId => Item.BillingNoteId;

    //El mes llega traducido desde el listado: la fila no lo resuelve
    public string PeriodText { get; }

    public string DateBillText => Item.DateBill.ToString("dd/MM/yyyy");

    public bool IsCreated => Item.Created;

    // Lanzada = congelada: sin editar y sin borrar
    public bool IsPending => !Item.Created;

    public string DateCreatedText => Item.DateCreated?.ToString("dd/MM/yyyy") ?? string.Empty;

    public BillingNoteRow(BillingNoteEntity item, string monthName)
    {
        Item = item;
        PeriodText = $"{monthName} {item.YearNumber}";
    }
}

// Nueva nota general: solo el periodo. La nota nace SIN lanzar.
public partial class CreateBillingNoteDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnotes";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;

    //El ultimo mes valido, para descartar la opcion neutra del combo
    private int _lastMonth = (int)MonthType.January;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private DateTime _dateBill = DateTime.Today;

    [ObservableProperty]
    private int _yearNumber = DateTime.Today.Year;

    [ObservableProperty]
    private int _selectedMonth = DateTime.Today.Month;

    [ObservableProperty]
    private bool _isLoading;

    public CreateBillingNoteDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _lastMonth = DateTime.Today.Month;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Months = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Mover la fecha ARRASTRA el periodo, igual que en la web
    partial void OnDateBillChanged(DateTime value)
    {
        YearNumber = value.Year;
        SelectedMonth = value.Month;
    }

    // La opcion neutra [Seleccione Mes] vale 0 y no es un mes: se vuelve al anterior
    partial void OnSelectedMonthChanged(int value)
    {
        if (Enum.IsDefined(typeof(MonthType), value))
        {
            _lastMonth = value;
            return;
        }

        SelectedMonth = _lastMonth;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsLoading = true;

        try
        {
            //Created y DateCreated los fuerza el servicio: la nota nace sin lanzar
            var model = new BillingNoteEntity
            {
                DateBill = DateBill,
                YearNumber = YearNumber,
                MonthType = (MonthType)SelectedMonth
            };

            var response = await _repository.PostAsync(BaseUrl, model);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
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

// Editar la nota general: solo el periodo, y solo mientras NO este lanzada.
public partial class EditBillingNoteDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnotes";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;

    private BillingNoteEntity? _model;
    private int _lastMonth = (int)MonthType.January;

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _months = new();

    [ObservableProperty]
    private DateTime _dateBill = DateTime.Today;

    [ObservableProperty]
    private int _yearNumber = DateTime.Today.Year;

    [ObservableProperty]
    private int _selectedMonth = DateTime.Today.Month;

    [ObservableProperty]
    private string _createdText = "No";

    [ObservableProperty]
    private string _dateCreatedText = string.Empty;

    // Lanzada = congelada. Se abre igual, pero sin poder guardar.
    [ObservableProperty]
    private bool _canEdit = true;

    [ObservableProperty]
    private bool _isLoading;

    public EditBillingNoteDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
    }

    public async Task InitializeAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            var meses = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
            if (await _responseHandler.HandleErrorAsync(meses))
            {
                return;
            }

            Months = new ObservableCollection<IntItemModel>(meses.Response ?? new List<IntItemModel>());

            var response = await _repository.GetAsync<BillingNoteEntity>($"{BaseUrl}/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            _model = response.Response;
            if (_model is null)
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            DateBill = _model.DateBill;
            YearNumber = _model.YearNumber;
            SelectedMonth = (int)_model.MonthType;
            _lastMonth = SelectedMonth;

            CreatedText = _model.Created ? "Si" : "No";
            DateCreatedText = _model.DateCreated?.ToString("dd/MM/yyyy") ?? string.Empty;
            CanEdit = !_model.Created;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnDateBillChanged(DateTime value)
    {
        YearNumber = value.Year;
        SelectedMonth = value.Month;
    }

    partial void OnSelectedMonthChanged(int value)
    {
        if (Enum.IsDefined(typeof(MonthType), value))
        {
            _lastMonth = value;
            return;
        }

        SelectedMonth = _lastMonth;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_model is null)
        {
            return;
        }

        IsLoading = true;

        try
        {
            //El servicio solo copia estos tres campos; el resto viaja para no perderlo
            _model.DateBill = DateBill;
            _model.YearNumber = YearNumber;
            _model.MonthType = (MonthType)SelectedMonth;

            var response = await _repository.PutAsync(BaseUrl, _model);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
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

// El detalle de la nota general y, sobre todo, su LANZAMIENTO.
//
// El lanzamiento son tres pasos y en este orden: revisar (check), mandar los contratos en
// lotes de 25 (launch/batch) y cerrar la nota (launch/finish). Cada lote es su propia
// transaccion, asi que una caida a mitad de camino deja guardado lo ya facturado y la nota
// sigue abierta: al reintentar, esos contratos se saltan solos.
//
// El endpoint de un solo golpe (POST {id}/launch) existe pero NO se usa: no revisa que el
// contrato este completo y facturaria contratos sin IP, sin MAC, sin queue ni binding.
public partial class BillingNoteDetailDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/billingnotes";

    //Los dos consecutivos por contrato bloquean la misma fila de Registers dentro de la
    //transaccion del lote: el tamano no se sube a la ligera
    private const int BatchSize = 25;

    //Cuantos problemas se alcanzan a leer en el resumen final
    private const int MaxIssuesShown = 10;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    private Guid _id;
    private List<BillingCheckDto> _checks = new();

    [ObservableProperty]
    private string _dateBillText = string.Empty;

    [ObservableProperty]
    private string _periodText = string.Empty;

    [ObservableProperty]
    private string _createdText = "No";

    [ObservableProperty]
    private string _dateCreatedText = string.Empty;

    // Sin lanzar es lo unico que habilita el boton Lanzar
    [ObservableProperty]
    private bool _canLaunch;

    //===== La revision, que solo aparece DESPUES de pulsar Lanzar =====
    [ObservableProperty]
    private bool _hasChecks;

    [ObservableProperty]
    private string _checkSummaryText = string.Empty;

    [ObservableProperty]
    private bool _allGood;

    [ObservableProperty]
    private ObservableCollection<BillingCheckRow> _incomplete = new();

    //===== La barra de avance =====
    [ObservableProperty]
    private bool _showProgress;

    [ObservableProperty]
    private int _processed;

    [ObservableProperty]
    private int _toProcess;

    [ObservableProperty]
    private bool _isLaunching;

    [ObservableProperty]
    private bool _isLoading;

    public int Percent => ToProcess == 0 ? 0 : Math.Min(100, Processed * 100 / ToProcess);

    public string ProgressText => $"Procesando {Processed} de {ToProcess}";

    public string PercentText => $"{Percent}%";

    public BillingNoteDetailDialogViewModel(
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
            var meses = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
            if (await _responseHandler.HandleErrorAsync(meses))
            {
                return;
            }

            var response = await _repository.GetAsync<BillingNoteEntity>($"{BaseUrl}/{id}");
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

            var mes = (meses.Response ?? new List<IntItemModel>())
                .FirstOrDefault(x => x.Value == (int)model.MonthType);

            DateBillText = model.DateBill.ToString("dd/MM/yyyy");
            PeriodText = $"{(string.IsNullOrWhiteSpace(mes?.Name) ? model.MonthType.ToString() : mes!.Name)} {model.YearNumber}";
            CreatedText = model.Created ? "Si" : "No";
            DateCreatedText = model.DateCreated?.ToString("dd/MM/yyyy") ?? string.Empty;
            CanLaunch = !model.Created;
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnProcessedChanged(int value)
    {
        OnPropertyChanged(nameof(Percent));
        OnPropertyChanged(nameof(PercentText));
        OnPropertyChanged(nameof(ProgressText));
    }

    partial void OnToProcessChanged(int value)
    {
        OnPropertyChanged(nameof(Percent));
        OnPropertyChanged(nameof(PercentText));
        OnPropertyChanged(nameof(ProgressText));
    }

    [RelayCommand]
    private async Task LaunchAsync()
    {
        if (IsLaunching)
        {
            return;
        }

        IsLaunching = true;
        Processed = 0;
        ToProcess = 0;
        ShowProgress = false;

        try
        {
            //Paso 1: la revision, que dice que contratos activos estan incompletos
            var revision = await _repository.GetAsync<List<BillingCheckDto>>($"{BaseUrl}/{_id}/check");
            if (await _responseHandler.HandleErrorAsync(revision))
            {
                return;
            }

            _checks = revision.Response ?? new List<BillingCheckDto>();
            MostrarRevision();

            var pendientes = _checks
                .Where(x => !x.AlreadyBilled && Faltantes(x).Count == 0)
                .Select(x => x.ContractClientId)
                .ToList();

            var bloqueados = _checks.Count(x => !x.AlreadyBilled && Faltantes(x).Count > 0);

            if (pendientes.Count == 0)
            {
                await _alertService.WarningAsync(
                    "Lanzar",
                    "No hay nada que lanzar: todos los contratos activos ya tienen su nota de este periodo.");
                return;
            }

            var texto = $"Se van a facturar {pendientes.Count} contrato(s).";

            if (bloqueados > 0)
            {
                texto += $" Quedan fuera {bloqueados} por estar incompletos.";
            }

            var confirmado = await _alertService.ConfirmAsync("Lanzar", texto, "Lanzar");
            if (!confirmado)
            {
                return;
            }

            //Paso 2: los lotes. Cada uno es su propia transaccion en el servidor.
            ToProcess = pendientes.Count;
            ShowProgress = true;

            var creadas = 0;
            var saltadas = 0;
            var fuera = new List<BillingLaunchIssueDto>();

            for (var i = 0; i < pendientes.Count; i += BatchSize)
            {
                var lote = pendientes.Skip(i).Take(BatchSize).ToList();

                var response = await _repository.PostAsync<List<Guid>, BillingLaunchResultDto>(
                    $"{BaseUrl}/{_id}/launch/batch", lote);

                if (await _responseHandler.HandleErrorAsync(response))
                {
                    //Lo ya facturado quedo guardado: no se cierra la nota y se avisa
                    await MostrarResumenAsync(creadas, saltadas, fuera, completo: false);
                    return;
                }

                var parcial = response.Response ?? new BillingLaunchResultDto();

                creadas += parcial.Created;
                saltadas += parcial.Skipped;
                fuera.AddRange(parcial.Issues);

                //La barra avanza de a lote, no de a contrato
                Processed += lote.Count;
            }

            //Paso 3: cerrar la nota. Solo aqui queda marcada como lanzada.
            var cierre = await _repository.PostAsync($"{BaseUrl}/{_id}/launch/finish", new { });
            if (await _responseHandler.HandleErrorAsync(cierre))
            {
                return;
            }

            await MostrarResumenAsync(creadas, saltadas, fuera, completo: true);
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

    private void MostrarRevision()
    {
        var incompletos = _checks
            .Where(x => !x.AlreadyBilled && Faltantes(x).Count > 0)
            .Select(x => new BillingCheckRow(x, string.Join(", ", Faltantes(x))))
            .ToList();

        var facturados = _checks.Count(x => x.AlreadyBilled);

        CheckSummaryText = $"{_checks.Count} activos - {incompletos.Count} incompletos - {facturados} ya facturados";
        Incomplete = new ObservableCollection<BillingCheckRow>(incompletos);
        AllGood = incompletos.Count == 0;
        HasChecks = true;
    }

    // Los SIETE bloquean: si falta cualquiera, ese contrato no se factura. Los literales y
    // su orden son los mismos del backend (BillingService.IncompleteContractsAsync) y hay
    // que mantenerlos iguales.
    private static List<string> Faltantes(BillingCheckDto item)
    {
        var faltas = new List<string>();

        if (!item.HasPlan)
        {
            faltas.Add("Plan");
        }

        if (!item.HasIp)
        {
            faltas.Add("IP");
        }

        if (!item.HasMac)
        {
            faltas.Add("MAC");
        }

        if (!item.HasServer)
        {
            faltas.Add("Servidor");
        }

        if (!item.HasNode)
        {
            faltas.Add("Nodo");
        }

        if (!item.HasQueue)
        {
            faltas.Add("Queue");
        }

        if (!item.HasBinding)
        {
            faltas.Add("IpBinding");
        }

        return faltas;
    }

    private async Task MostrarResumenAsync(
        int creadas,
        int saltadas,
        List<BillingLaunchIssueDto> fuera,
        bool completo)
    {
        var lineas = new List<string>
        {
            $"Notas creadas: {creadas}",
            $"Ya facturados, se saltaron: {saltadas}"
        };

        if (fuera.Count > 0)
        {
            lineas.Add($"Quedaron fuera ({fuera.Count}):");
            lineas.AddRange(fuera
                .Take(MaxIssuesShown)
                .Select(x => $"#{x.ControlContrato} {x.ClientFullName}: {x.Reason}"));
        }

        if (!completo)
        {
            lineas.Add("El lanzamiento se interrumpio. Lo facturado quedo guardado y la nota sigue abierta: vuelva a lanzarla para continuar.");
        }

        var texto = string.Join(Environment.NewLine, lineas);

        if (completo && fuera.Count == 0)
        {
            await _alertService.SuccessAsync("Notas lanzadas", texto);
            return;
        }

        await _alertService.WarningAsync(
            completo ? "Notas lanzadas" : "Lanzamiento interrumpido",
            texto);
    }
}

// Un contrato activo que NO se puede facturar, con lo que le falta
public class BillingCheckRow
{
    public BillingCheckDto Item { get; }

    public string Title => $"#{Item.ControlContrato} {Item.ClientFullName}";

    public string? ZoneName => Item.ZoneName;

    public string MissingText { get; }

    public BillingCheckRow(BillingCheckDto item, string missingText)
    {
        Item = item;
        MissingText = missingText;
    }
}
