using Spix.AppWpf.ViewModels.EntitiesBilling.Sell;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.Sell;

// Facturas. Al abrir se pide primero el tablero del mes y despues la primera pagina.
public partial class SellIndexView : UserControl
{
    private readonly SellIndexViewModel _viewModel;
    private bool _isLoaded;

    public SellIndexView(SellIndexViewModel viewModel)
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
