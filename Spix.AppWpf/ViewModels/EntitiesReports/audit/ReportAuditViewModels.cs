using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesReports.audit;

// Bitacora del dinero: cada movimiento que quedo anotado, del mas nuevo al mas viejo.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no toca el MikroTik. Solo lee lo que
// el backend ya dejo escrito en la bitacora de pagos.
public partial class ReportAuditIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v3/reports-finance";
    private const int PageSize = 20;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ReportAuditRow> _rows = new();

    //La lista la arma el backend con el neutro Todos en la posicion 0: aqui no se toca
    [ObservableProperty]
    private ObservableCollection<IntItemModel> _events = new();

    //Arranca con el mes en curso y con todos los movimientos, igual que en la web
    [ObservableProperty]
    private DateTime _dateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private int _eventType;

    [ObservableProperty]
    private string? _filter;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    public ReportAuditIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Primero los tipos de movimiento y despues la primera busqueda, como en la web
    public async Task InitializeAsync()
    {
        var response = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/audit/comboevents");
        if (!await _responseHandler.HandleErrorAsync(response))
        {
            Events = new ObservableCollection<IntItemModel>(response.Response ?? new List<IntItemModel>());
        }

        //Aunque falle la lista de tipos la bitacora se muestra: el neutro es Todos
        await LoadAsync(1);
    }

    // Cambiar el tipo de movimiento vuelve a preguntar desde la primera pagina
    partial void OnEventTypeChanged(int value) => _ = LoadAsync(1);

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

    private async Task LoadAsync(int page)
    {
        IsLoading = true;

        try
        {
            var url = $"{BaseUrl}/audit?page={page}&recordsnumber={PageSize}&eventtype={EventType}" +
                      $"&datestart={DateStart:yyyy-MM-dd}&dateend={DateEnd:yyyy-MM-dd}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
            }

            var response = await _repository.GetAsync<List<ReportAuditDto>>(url);

            //Si falla se deja en pantalla lo que ya se estaba viendo
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Rows = new ObservableCollection<ReportAuditRow>(
                (response.Response ?? new List<ReportAuditDto>()).Select(x => new ReportAuditRow(x)));

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
}

// Un movimiento de la bitacora, ya listo para pintar
public class ReportAuditRow
{
    public ReportAuditDto Item { get; }

    public string DateEventText => Item.DateEvent.ToString("dd/MM/yyyy");

    //La hora va aparte porque en la bitacora importa el orden dentro del mismo dia
    public string TimeEventText => Item.DateEvent.ToString("HH:mm");

    //El nombre del movimiento ya viene traducido del backend
    public string? EventName => Item.EventName;

    public string? ClientFullName => Item.ClientFullName;

    public string? Detail => Item.Detail;

    public string? UserByName => Item.UserByName;

    public decimal Total => Item.Total;

    public ReportAuditRow(ReportAuditDto item)
    {
        Item = item;
    }
}
