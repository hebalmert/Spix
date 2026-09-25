using Spix.AppWpf.ViewModels.EntitiesSchedule.ServiceRequest;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule.ServiceRequest;

// Solicitudes de servicio. Al abrir se piden las pildoras, el tablero y la primera pagina.
public partial class ServiceRequestIndexView : UserControl
{
    private readonly ServiceRequestIndexViewModel _viewModel;
    private bool _isLoaded;

    public ServiceRequestIndexView(ServiceRequestIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

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
}
