using CommunityToolkit.Mvvm.ComponentModel;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesNet.OltMap;

// El mapa de OLT, replicado de la pantalla /oltmap de la web.
//
// Al entrar pinta TODAS las OLT con su nombre y cuantos clientes tiene cada una; al elegir
// una, pinta sus clientes con la distancia al equipo. Volver al neutro regresa a la general.
//
// Las dos listas (OLT y vistas) llegan ARMADAS del backend, con su elemento neutro ya
// traducido: aqui no se filtra, no se ordena y no se agrega ninguna opcion.
public partial class OltMapViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/oltmap";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<GuidNameModel> _olts = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _views = new();

    //Las dos vistas: Map con una OLT elegida, AllOlts cuando esta el neutro
    [ObservableProperty]
    private OltMapDto? _map;

    [ObservableProperty]
    private ObservableCollection<OltMapItemDto> _allOlts = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private Guid _selectedUnlocatedId;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool HasMap => Map is not null;

    //El tablero de la vista de todas
    public int AllCount => AllOlts.Count;

    public int AllWithPoint => AllOlts.Count(x => x.Latitude.HasValue && x.Longitude.HasValue);

    public int AllWithoutPoint => AllCount - AllWithPoint;

    public int AllClients => AllOlts.Sum(x => x.Clients);

    private Guid _selectedOltId;

    public Guid SelectedOltId
    {
        get => _selectedOltId;
        set
        {
            if (_selectedOltId == value)
            {
                return;
            }

            _selectedOltId = value;
            OnPropertyChanged();

            _ = LoadMapAsync();
        }
    }

    private int _selectedView = 1;

    // Como se ve: solo puntos, lineas a la OLT, o lineas con la distancia encima
    public int SelectedView
    {
        get => _selectedView;
        set
        {
            if (_selectedView == value)
            {
                return;
            }

            _selectedView = value;
            OnPropertyChanged();

            ViewChanged?.Invoke(this, value);
        }
    }

    // La pantalla escucha estos avisos para hablarle al mapa, que vive en el navegador
    public event EventHandler<OltMapDto>? MapLoaded;

    public event EventHandler<IReadOnlyList<OltMapItemDto>>? AllLoaded;

    public event EventHandler<int>? ViewChanged;

    public OltMapViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    partial void OnMapChanged(OltMapDto? value)
    {
        OnPropertyChanged(nameof(HasMap));
    }

    partial void OnAllOltsChanged(ObservableCollection<OltMapItemDto> value)
    {
        OnPropertyChanged(nameof(AllCount));
        OnPropertyChanged(nameof(AllWithPoint));
        OnPropertyChanged(nameof(AllWithoutPoint));
        OnPropertyChanged(nameof(AllClients));
    }

    // Las dos listas se piden una sola vez al abrir la pantalla, y de una se pinta la general
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var olts = await _repository.GetAsync<List<GuidNameModel>>($"{BaseUrl}/olts");
            if (await _responseHandler.HandleErrorAsync(olts))
            {
                return;
            }

            Olts = new ObservableCollection<GuidNameModel>(olts.Response ?? new List<GuidNameModel>());

            var vistas = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/views");
            if (await _responseHandler.HandleErrorAsync(vistas))
            {
                return;
            }

            Views = new ObservableCollection<IntItemModel>(vistas.Response ?? new List<IntItemModel>());

            await LoadAllAsync();
        }
        catch (Exception exception)
        {
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Todas las OLT: un solo request, y el conteo de clientes ya viene hecho por la base
    private async Task LoadAllAsync()
    {
        var responseHttp = await _repository.GetAsync<List<OltMapItemDto>>($"{BaseUrl}/all");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        AllOlts = new ObservableCollection<OltMapItemDto>(responseHttp.Response ?? new List<OltMapItemDto>());
        AllLoaded?.Invoke(this, AllOlts);
    }

    // Una OLT: un solo request con sus clientes, sus distancias y el tablero.
    // Con el neutro se vuelve a la vista de todas.
    private async Task LoadMapAsync()
    {
        IsLoading = true;
        Message = string.Empty;

        try
        {
            SelectedUnlocatedId = Guid.Empty;

            if (SelectedOltId == Guid.Empty)
            {
                Map = null;
                await LoadAllAsync();
                return;
            }

            var responseHttp = await _repository.GetAsync<OltMapDto>($"{BaseUrl}/{SelectedOltId}");
            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            Map = responseHttp.Response;

            if (Map is not null)
            {
                MapLoaded?.Invoke(this, Map);
            }
        }
        catch (Exception exception)
        {
            Map = null;
            Message = exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
