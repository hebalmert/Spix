using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesNet.NodeMap;

// El mapa de nodos, replicado de la pantalla /nodemap de la web.
//
// Las tres listas (nodos, vistas, coberturas) llegan ARMADAS del backend, con su elemento
// neutro ya traducido: aqui no se filtra, no se ordena y no se agrega ninguna opcion.
public partial class NodeMapViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/nodemap";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<GuidNameModel> _nodes = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _views = new();

    [ObservableProperty]
    private ObservableCollection<IntItemModel> _coverages = new();

    [ObservableProperty]
    private NodeMapDto? _map;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public bool HasMap => Map is not null;

    //Los clientes del nodo que solo se sabe que salen por el AP, sin coordenadas
    [ObservableProperty]
    private Guid _selectedUnlocatedId;

    private Guid _selectedNodeId;

    public Guid SelectedNodeId
    {
        get => _selectedNodeId;
        set
        {
            if (_selectedNodeId == value)
            {
                return;
            }

            _selectedNodeId = value;
            OnPropertyChanged();

            _ = LoadMapAsync();
        }
    }

    private int _selectedView = 1;

    // Como se ve: solo puntos, lineas al AP, o lineas con la distancia encima
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

    private int _selectedCoverage;

    // El ancho de la mascara del transmisor, en grados. Con 0 no se pinta nada.
    public int SelectedCoverage
    {
        get => _selectedCoverage;
        set
        {
            if (_selectedCoverage == value)
            {
                return;
            }

            _selectedCoverage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCoverage));
            OnPropertyChanged(nameof(InsideCoverage));

            AvisarCobertura();
        }
    }

    private int _azimuth;

    // Hacia donde apunta el transmisor, en grados
    public int Azimuth
    {
        get => _azimuth;
        set
        {
            var grados = Math.Clamp(value, 0, 359);

            if (_azimuth == grados)
            {
                return;
            }

            _azimuth = grados;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InsideCoverage));

            AvisarCobertura();
        }
    }

    public bool HasCoverage => SelectedCoverage > 0;

    // Los clientes que caen dentro del sector: se sabe con el rumbo que calculo el backend
    public int InsideCoverage
    {
        get
        {
            if (Map is null || SelectedCoverage <= 0)
            {
                return 0;
            }

            var mitad = SelectedCoverage / 2d;

            return Map.Located.Count(x => x.BearingDeg.HasValue && Diferencia(x.BearingDeg.Value, Azimuth) <= mitad);
        }
    }

    // La pantalla escucha estos avisos para hablarle al mapa, que vive en el navegador
    public event EventHandler<NodeMapDto>? MapLoaded;

    public event EventHandler<int>? ViewChanged;

    public event EventHandler<(int Degrees, int Azimuth, double RadiusKm)>? CoverageChanged;

    public NodeMapViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    partial void OnMapChanged(NodeMapDto? value)
    {
        OnPropertyChanged(nameof(HasMap));
        OnPropertyChanged(nameof(InsideCoverage));
    }

    // Las tres listas se piden una sola vez al abrir la pantalla
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var nodos = await _repository.GetAsync<List<GuidNameModel>>($"{BaseUrl}/nodes");
            if (await _responseHandler.HandleErrorAsync(nodos))
            {
                return;
            }

            Nodes = new ObservableCollection<GuidNameModel>(nodos.Response ?? new List<GuidNameModel>());

            var vistas = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/views");
            if (await _responseHandler.HandleErrorAsync(vistas))
            {
                return;
            }

            Views = new ObservableCollection<IntItemModel>(vistas.Response ?? new List<IntItemModel>());

            var coberturas = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/coverages");
            if (await _responseHandler.HandleErrorAsync(coberturas))
            {
                return;
            }

            Coverages = new ObservableCollection<IntItemModel>(coberturas.Response ?? new List<IntItemModel>());
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

    [RelayCommand]
    private void StepAzimuth(string? paso)
    {
        //Los botones mueven de a un grado: con el deslizador cuesta afinar
        if (int.TryParse(paso, out var grados))
        {
            Azimuth += grados;
        }
    }

    private async Task LoadMapAsync()
    {
        if (SelectedNodeId == Guid.Empty)
        {
            Map = null;
            return;
        }

        IsLoading = true;
        Message = string.Empty;

        try
        {
            var responseHttp = await _repository.GetAsync<NodeMapDto>($"{BaseUrl}/{SelectedNodeId}");
            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            Map = responseHttp.Response;
            SelectedUnlocatedId = Guid.Empty;

            if (Map is not null)
            {
                MapLoaded?.Invoke(this, Map);
                AvisarCobertura();
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

    private void AvisarCobertura()
    {
        if (Map is null)
        {
            return;
        }

        //El radio del sector: un poco mas que el cliente mas lejano, para que se vea entero
        var radio = Math.Max(0.5, (Map.FarthestKm ?? 0) * 1.15);

        CoverageChanged?.Invoke(this, (SelectedCoverage, Azimuth, radio));
    }

    // Cuantos grados separan dos rumbos, por el lado corto de la brujula
    private static double Diferencia(double rumbo, double azimut)
    {
        var diferencia = Math.Abs(rumbo - azimut) % 360;

        return diferencia > 180 ? 360 - diferencia : diferencia;
    }
}
