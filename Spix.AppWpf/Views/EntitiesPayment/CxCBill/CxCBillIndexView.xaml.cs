using Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.CxCBill;

// Cuentas por cobrar. Al abrir se piden el tablero y la primera pagina.
public partial class CxCBillIndexView : UserControl
{
    private readonly CxCBillIndexViewModel _viewModel;
    private bool _isLoaded;

    public CxCBillIndexView(CxCBillIndexViewModel viewModel)
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
