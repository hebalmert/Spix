using Spix.AppWpf.ViewModels.EntitiesContratos.Activation;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.Activation;

// Reactivacion. Al abrir se piden el tablero y la primera pagina de los que esperan.
public partial class ActivationIndexView : UserControl
{
    private readonly ActivationIndexViewModel _viewModel;
    private bool _isLoaded;

    public ActivationIndexView(ActivationIndexViewModel viewModel)
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
