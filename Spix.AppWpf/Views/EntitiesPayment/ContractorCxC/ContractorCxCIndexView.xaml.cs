using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;

// Pagos a contratistas. Al abrir se piden el tablero y la primera pagina.
public partial class ContractorCxCIndexView : UserControl
{
    private readonly ContractorCxCIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractorCxCIndexView(ContractorCxCIndexViewModel viewModel)
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
