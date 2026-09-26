using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.activecontracts;

// Reporte de contratos activos: quien esta conectado, con que plan y cuanto paga.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Por eso las filas
// no llevan botones.
//
// El tablero y la tabla salen del MISMO estado elegido en el combo, asi que los numeros de
// arriba si cuadran con lo que se lista abajo; lo unico que no entra al tablero es el texto
// del buscador, que solo recorta la tabla. Es asi tambien en la web.
public partial class ReportActiveContractsIndexViewModel : ObservableObject
{
    //Los reportes viven en su propia version del API
    private const string BaseUrl = "api/v3/reports";

    //La web pagina de 20 en 20 en este reporte: se respeta para que las paginas coincidan
    private const int PageSize = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    //Mientras se arma la pantalla el combo cambia solo; sin esta bandera dispararia una
    //consulta de mas antes de la primera carga
    private bool _isReady;

    [ObservableProperty]
    private ObservableCollection<ReportActiveContractsRow> _rows = new();

    //La lista la arma el backend con su neutro en la posicion 0: aqui no se filtra ni se ordena
    [ObservableProperty]
    private ObservableCollection<IntItemModel> _states = new();

    //Arranca mostrando los activos, igual que la web
    [ObservableProperty]
    private int _stateId = (int)ContractState.Active;

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
    private int _contracts;

    [ObservableProperty]
    private decimal _monthlyTotal;

    [ObservableProperty]
    private int _withoutPlan;

    public string EmptyText => "No hay contratos para este estado.";

    public ReportActiveContractsIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Primero el combo, despues el tablero y por ultimo la tabla: asi el combo ya puede
    // mostrar el estado con el que arranca el reporte
    public async Task InitializeAsync()
    {
        await LoadStatesAsync();
        await LoadSummaryAsync();
        await LoadAsync(1);

        _isReady = true;
    }

    private async Task LoadStatesAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combocontractstates");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        States = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
    }

    // Si el tablero falla la pantalla NO se cae: simplemente no se pinta
    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<ReportActiveSummaryDto>($"{BaseUrl}/active/summary?stateid={StateId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            HasSummary = false;
            return;
        }

        var datos = response.Response ?? new ReportActiveSummaryDto();

        Contracts = datos.Contracts;
        MonthlyTotal = datos.MonthlyTotal;
        WithoutPlan = datos.WithoutPlan;
        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}/active?page={page}&recordsnumber={PageSize}&stateid={StateId}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            var response = await _repository.GetAsync<List<ReportActiveContractDto>>(url);

            //Si falla se deja en pantalla lo que ya se estaba viendo
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ReportActiveContractsRow>(
                (response.Response ?? new List<ReportActiveContractDto>()).Select(x => new ReportActiveContractsRow(x)));

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

    // Cambiar el estado cambia la pregunta entera: tablero y tabla vuelven a preguntar, y se
    // regresa a la primera pagina porque la anterior ya no significa lo mismo
    partial void OnStateIdChanged(int value)
    {
        if (!_isReady)
        {
            return;
        }

        _ = RecargarPorEstadoAsync();
    }

    private async Task RecargarPorEstadoAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(1);
    }

    // Buscar solo recorta la tabla: el tablero sigue siendo el del estado completo
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
    private async Task RefreshAsync()
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage < 1 ? 1 : CurrentPage);
    }

    // Paginar NO vuelve a pedir el tablero: los numeros no cambian entre paginas
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }
}

// Un contrato del reporte, ya listo para pintar: el XAML no calcula nada
public class ReportActiveContractsRow
{
    public ReportActiveContractDto Item { get; }

    public string ClientFullName => Item.ClientFullName;

    public string ContractText => $"Contrato #{Item.ControlContrato}";

    public string? ZoneName => Item.ZoneName;

    public string? ServerName => Item.ServerName;

    public string? PlanName => Item.PlanName;

    public decimal PlanPrice => Item.PlanPrice;

    // La pastilla solo aparece cuando el contrato NO esta activo: cuando el combo muestra
    // todos los estados es la unica senal de que esa fila ya no esta conectada
    public bool ShowInactive => !Item.IsActive;

    public ReportActiveContractsRow(ReportActiveContractDto item)
    {
        Item = item;
    }
}
