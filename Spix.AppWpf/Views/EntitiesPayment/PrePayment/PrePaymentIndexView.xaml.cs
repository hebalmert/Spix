using Spix.AppWpf.ViewModels.EntitiesPayment.PrePayment;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.PrePayment;

// Pagos adelantados. Al abrir se bajan los meses, el tablero y la primera pagina.
public partial class PrePaymentIndexView : UserControl
{
    private readonly PrePaymentIndexViewModel _viewModel;
    private bool _isLoaded;

    public PrePaymentIndexView(PrePaymentIndexViewModel viewModel)
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
