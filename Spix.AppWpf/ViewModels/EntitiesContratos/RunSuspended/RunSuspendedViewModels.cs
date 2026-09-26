using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.RunSuspended;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using RunSuspendedEntity = Spix.Domain.EntitiesContratos.RunSuspended;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.RunSuspended;

// Corte general: suspende de un golpe los contratos que deben, equipo por equipo.
//
// El corte se identifica por ano y mes, pero eso es SOLO una etiqueta del periodo: no
// filtra la deuda por mes. Toma cualquier nota no anulada con saldo, del mes que sea, para
// que nadie quede navegando debiendo.
//
// La ejecucion vive en el detalle, que es donde esta el trabajo de verdad.
public partial class RunSuspendedIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/runsuspended";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<RunSuspendedRow> _rows = new();

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
    private int _suspended;

    [ObservableProperty]
    private int _debtors;

    [ObservableProperty]
    private decimal _debtTotal;

    [ObservableProperty]
    private int _pendingRuns;

    //Los nombres de los meses los arma el backend: aqui solo se pintan
    private List<IntItemModel> _months = new();

    public RunSuspendedIndexViewModel(
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

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<CorteSummaryDto>($"{BaseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new CorteSummaryDto();

        Suspended = datos.Suspended;
        Debtors = datos.Debtors;
        DebtTotal = datos.DebtTotal;
        PendingRuns = datos.Pending;
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

            var response = await _repository.GetAsync<List<RunSuspendedEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<RunSuspendedRow>(
                (response.Response ?? new List<RunSuspendedEntity>()).Select(x => new RunSuspendedRow(x, NombreDelMes)));

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

    private string NombreDelMes(MonthType mes) =>
        _months.FirstOrDefault(x => x.Value == (int)mes)?.Name ?? mes.ToString();

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

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<RunSuspendedFormDialogView>("Nuevo corte general");

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // Un corte ya ejecutado no se edita
    [RelayCommand]
    private async Task EditAsync(RunSuspendedRow? fila)
    {
        if (fila is null || !fila.CanEdit)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.RunSuspendedId
        };

        var result = await _modalService.ShowAsync<RunSuspendedFormDialogView>("Editar corte general", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    // El detalle es donde se revisa y se ejecuta el corte
    [RelayCommand]
    private async Task DetailAsync(RunSuspendedRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.RunSuspendedId
        };

        var result = await _modalService.ShowAsync<RunSuspendedDetailDialogView>("Detalle corte general", parametros);

        if (result.Succeeded)
        {
            await RecargarAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(RunSuspendedRow? fila)
    {
        if (fila is null || !fila.CanEdit)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar",
            "Desea eliminar este corte general?",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.RunSuspendedId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await RecargarAsync();
        await _alertService.SuccessAsync("Eliminado", "Registro eliminado correctamente.");
    }
}

// Una fila de la tabla, ya lista para pintar
public class RunSuspendedRow
{
    public RunSuspendedEntity Item { get; }

    public Guid RunSuspendedId => Item.RunSuspendedId;

    public string PeriodText { get; }

    //Vacio mientras no se ejecute
    public string DateText => Item.DateUtc?.ToString("dd/MM/yyyy") ?? string.Empty;

    public string? UserByName => Item.UserByName;

    public string StatusText => Item.Executed ? "Ejecutado" : "Pendiente";

    public Brush StatusColor => Application.Current.TryFindResource(
        Item.Executed ? "BrushCatalogKpiBad" : "BrushCatalogKpiWarn") as Brush ?? Brushes.Gray;

    //Un corte ya ejecutado no se toca: ni se edita ni se elimina
    public bool CanEdit => !Item.Executed;

    public RunSuspendedRow(RunSuspendedEntity item, Func<MonthType, string> nombreDelMes)
    {
        Item = item;
        PeriodText = $"{nombreDelMes(item.MonthType)} {item.YearNumber}";
    }
}
