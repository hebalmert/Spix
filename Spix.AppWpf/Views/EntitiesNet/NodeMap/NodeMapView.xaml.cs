using Spix.AppWpf.ViewModels.EntitiesNet.NodeMap;
using Spix.DomainLogic.EntitiesNetDTO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Spix.AppWpf.Views.EntitiesNet.NodeMap;

// El mapa vive dentro de un navegador, asi que no se puede enlazar con Binding: la
// pantalla escucha los avisos del ViewModel y le habla al visor.
public partial class NodeMapView : UserControl
{
    private readonly NodeMapViewModel _viewModel;
    private bool _isLoaded;

    public NodeMapView(NodeMapViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.MapLoaded += MapaCargado;
        _viewModel.ViewChanged += VistaCambiada;
        _viewModel.CoverageChanged += CoberturaCambiada;

        Loaded += CargarPantalla;
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }

    private void MapaCargado(object? sender, NodeMapDto mapa)
    {
        DespuesDeAcomodar(() => MapViewer.RenderAsync(mapa, _viewModel.SelectedView));
    }

    private void VistaCambiada(object? sender, int vista)
    {
        DespuesDeAcomodar(() => MapViewer.SetViewAsync(vista));
    }

    private void CoberturaCambiada(object? sender, (int Degrees, int Azimuth, double RadiusKm) cobertura)
    {
        DespuesDeAcomodar(() => MapViewer.SetCoverageAsync(cobertura.Degrees, cobertura.Azimuth, cobertura.RadiusKm));
    }

    // Le habla al mapa DESPUES de que la pantalla se acomode, igual que la web, que lo
    // dibuja en OnAfterRenderAsync "cuando el div ya tiene su tamano final".
    //
    // Hace falta porque el visor esta oculto mientras no hay nodo elegido: al elegirlo, en
    // ese mismo instante todavia mide cero y Leaflet montaria el mapa sobre un alto de
    // cero, con el encuadre calculado en vano.
    //
    // Las tres ordenes pasan por aqui a proposito: al elegir un nodo salen seguidas la de
    // dibujar y la de la mascara, y con la misma prioridad el Dispatcher las respeta en
    // orden. Si una se adelantara, la mascara llegaria antes que el mapa que la sostiene.
    private void DespuesDeAcomodar(Func<Task> orden)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(async () => await orden()));
    }
}
