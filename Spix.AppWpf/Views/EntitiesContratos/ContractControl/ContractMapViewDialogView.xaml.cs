using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.SharedServices;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// Ver la ubicacion del cliente en el mapa, con su nodo si lo tiene ubicado.
//
// No tiene ViewModel: no pide nada al servidor ni guarda nada. Recibe las coordenadas ya
// cargadas por el detalle y solo las pinta.
public partial class ContractMapViewDialogView : UserControl, ISharedModalContent
{
    private readonly ModalService _modalService;

    private decimal _latitude;
    private decimal _longitude;
    private decimal? _nodeLatitude;
    private decimal? _nodeLongitude;
    private string? _nodeName;
    private bool _loaded;

    public ContractMapViewDialogView(ModalService modalService)
    {
        InitializeComponent();

        _modalService = modalService;
        DataContext = this;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        if (parameters.TryGetValue("Latitude", out var lat) && lat is decimal latitud)
        {
            _latitude = latitud;
        }

        if (parameters.TryGetValue("Longitude", out var lng) && lng is decimal longitud)
        {
            _longitude = longitud;
        }

        //El nodo es opcional: solo si tiene coordenadas cargadas
        if (parameters.TryGetValue("NodeLatitude", out var nodeLat) && nodeLat is decimal nodeLatitud)
        {
            _nodeLatitude = nodeLatitud;
        }

        if (parameters.TryGetValue("NodeLongitude", out var nodeLng) && nodeLng is decimal nodeLongitud)
        {
            _nodeLongitude = nodeLongitud;
        }

        if (parameters.TryGetValue("NodeName", out var nombre) && nombre is string texto)
        {
            _nodeName = texto;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        await MapViewer.ShowMapAsync(
            _latitude,
            _longitude,
            "Cliente",
            _nodeLatitude,
            _nodeLongitude,
            _nodeName);
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
