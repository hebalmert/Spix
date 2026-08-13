using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Service;

// Muestra el indice paginado de categorias de servicios.
public partial class ServiceIndexView : UserControl
{
    private readonly ServiceIndexViewModel _viewModel;
    private bool _isLoaded;

    public ServiceIndexView(ServiceIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadView;
    }

    // Solicita registros solo la primera vez que la vista entra al contenedor principal.
    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
