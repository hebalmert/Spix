using Spix.AppWpf.ViewModels.EntitiesReports.activecontracts;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.activecontracts;

// Contratos activos. Al abrir se baja el combo de estados, luego el tablero y por ultimo la
// primera pagina.
public partial class ReportActiveContractsIndexView : UserControl
{
    private readonly ReportActiveContractsIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportActiveContractsIndexView(ReportActiveContractsIndexViewModel viewModel)
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
