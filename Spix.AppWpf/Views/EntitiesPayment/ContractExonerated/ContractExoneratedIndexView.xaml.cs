using Spix.AppWpf.ViewModels.EntitiesPayment.ContractExonerated;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;

// Exoneraciones programadas. Al abrir se piden los meses, el tablero y la primera pagina.
public partial class ContractExoneratedIndexView : UserControl
{
    private readonly ContractExoneratedIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractExoneratedIndexView(ContractExoneratedIndexViewModel viewModel)
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
