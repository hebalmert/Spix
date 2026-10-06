using Spix.AppWpf.ViewModels.EntitiesNet.OltMap;
using Spix.DomainLogic.EntitiesNetDTO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Spix.AppWpf.Views.EntitiesNet.OltMap;

// El mapa vive dentro de un navegador, asi que no se puede enlazar con Binding: la
// pantalla escucha los avisos del ViewModel y le habla al visor.
public partial class OltMapView : UserControl
{
    private readonly OltMapViewModel _viewModel;
    private bool _isLoaded;

    public OltMapView(OltMapViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.AllLoaded += TodasCargadas;
        _viewModel.MapLoaded += MapaCargado;
        _viewModel.ViewChanged += VistaCambiada;

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

    private void TodasCargadas(object? sender, IReadOnlyList<OltMapItemDto> olts)
    {
        DespuesDeAcomodar(() => MapViewer.RenderAllAsync(olts, "clientes"));
    }

    private void MapaCargado(object? sender, OltMapDto mapa)
    {
        DespuesDeAcomodar(() => MapViewer.RenderAsync(mapa, _viewModel.SelectedView));
    }

    private void VistaCambiada(object? sender, int vista)
    {
        DespuesDeAcomodar(() => MapViewer.SetViewAsync(vista));
    }

    // Le habla al mapa DESPUES de que la pantalla se acomode, igual que la web, que lo
    // dibuja en OnAfterRenderAsync "cuando el div ya tiene su tamano final".
    //
    // Todas las ordenes pasan por aqui a proposito y con la MISMA prioridad: el Dispatcher
    // las respeta en orden, y si una se adelantara llegaria antes que el mapa que la sostiene.
    private void DespuesDeAcomodar(Func<Task> orden)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(async () => await orden()));
    }
}
