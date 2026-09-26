using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesPayment.TechnicianCollection;

// Cobro tecnico: el cuadre con quien recoge la plata en la calle.
//
// Se elige una persona y un rango de fechas, y sale lo que recibio y cuanto suma: ese total
// es lo que la oficina tiene que recibirle.
//
// Es de SOLA LECTURA: no crea, no edita, no borra y no escribe ni una fila. Lee del unico
// lugar donde vive el dinero, los abonos de las notas de cobro, asi que nada se duplica.
public partial class TechnicianCollectionIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/technician-collections";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<TechnicianCollectionRow> _rows = new();

    //La lista la arma el backend con el neutro en la posicion 0: aqui no se toca
    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _collectors = new();

    [ObservableProperty]
    private Guid _userId;

    [ObservableProperty]
    private DateTime _dateStart = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime _dateEnd = DateTime.Today;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    //===== El cuadre =====
    [ObservableProperty]
    private bool _hasSummary;

    [ObservableProperty]
    private int _collections;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _cash;

    [ObservableProperty]
    private decimal _bank;

    public string EmptyText => "Elija un tecnico y un periodo, y pulse Buscar.";

    public TechnicianCollectionIndexViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    // Al abrir solo se baja la lista de quienes cobran: nada mas hasta que se busque
    public async Task InitializeAsync()
    {
        var response = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combocollectors");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Collectors = new ObservableCollection<GuidItemModel>(response.Response ?? new List<GuidItemModel>());
    }

    // Sin persona elegida no se busca nada, y sin mensaje: es el neutro de la lista
    [RelayCommand]
    private async Task SearchAsync()
    {
        if (UserId == Guid.Empty)
        {
            return;
        }

        IsLoading = true;

        try
        {
            CurrentPage = 1;

            await LoadSummaryAsync();
            await LoadAsync(1);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string Periodo() =>
        $"datestart={DateStart:yyyy-MM-dd}&dateend={DateEnd:yyyy-MM-dd}";

    private async Task LoadSummaryAsync()
    {
        var response = await _repository.GetAsync<TechnicianCollectionSummaryDto>(
            $"{BaseUrl}/summary/{UserId}?{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var datos = response.Response ?? new TechnicianCollectionSummaryDto();

        Collections = datos.Collections;
        Total = datos.Total;
        Cash = datos.Cash;

        //Tarjeta y transferencia se muestran juntas: nunca se ven por separado
        Bank = datos.Card + datos.Transfer;

        HasSummary = true;
    }

    private async Task LoadAsync(int page)
    {
        var response = await _repository.GetAsync<List<TechnicianCollectionDto>>(
            $"{BaseUrl}/{UserId}?page={page}&recordsnumber={PageSize}&{Periodo()}");

        //Si falla se deja lo que hubiera en pantalla
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        Rows = new ObservableCollection<TechnicianCollectionRow>(
            (response.Response ?? new List<TechnicianCollectionDto>()).Select(x => new TechnicianCollectionRow(x)));

        CurrentPage = page;

        if (response.HttpResponseMessage is not null &&
            response.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var valores) &&
            int.TryParse(valores.FirstOrDefault(), out var total))
        {
            TotalPages = total;
        }
    }

    // Paginar NO vuelve a pedir el cuadre: los numeros son los del momento de la busqueda
    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        await LoadAsync(page);
    }
}

// Un cobro recibido, ya listo para pintar
public class TechnicianCollectionRow
{
    public TechnicianCollectionDto Item { get; }

    public string DatePaymentText => Item.DatePayment.ToString("dd/MM/yyyy");

    public string? ClientFullName => Item.ClientFullName;

    public string NoteText => $"{Item.CollectionNote} · Contrato #{Item.ControlContrato}";

    // El modo llega como cadena del backend; solo se traducen estas tres
    public string ModeName => Item.PaymentMode switch
    {
        "Cash" => "Efectivo",
        "Card" => "Tarjeta",
        "Transfer" => "Transferencia",
        null => string.Empty,
        _ => Item.PaymentMode
    };

    public decimal Discount => Item.Discount;

    public decimal Payment => Item.Payment;

    public TechnicianCollectionRow(TechnicianCollectionDto item)
    {
        Item = item;
    }
}
