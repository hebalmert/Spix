using Spix.AppWpf.ViewModels.EntitiesContratos.RunSuspended;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.RunSuspended;

// Corte general. Al abrir se piden los meses, el tablero y la primera pagina.
public partial class RunSuspendedIndexView : UserControl
{
    private readonly RunSuspendedIndexViewModel _viewModel;
    private bool _isLoaded;

    public RunSuspendedIndexView(RunSuspendedIndexViewModel viewModel)
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
