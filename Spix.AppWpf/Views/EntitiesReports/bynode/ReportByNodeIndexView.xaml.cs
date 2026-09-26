using Spix.AppWpf.ViewModels.EntitiesReports.bynode;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.bynode;

// Contratos por AP. Al abrir solo se bajan los dos combos: el reporte espera a que se
// elija un AP.
public partial class ReportByNodeIndexView : UserControl
{
    private readonly ReportByNodeIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportByNodeIndexView(ReportByNodeIndexViewModel viewModel)
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
