using Spix.AppWpf.ViewModels.EntitiesReports.aging;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.aging;

// Cartera por antiguedad. Al abrir se baja el tablero y la lista de los que mas deben.
public partial class ReportAgingIndexView : UserControl
{
    private readonly ReportAgingIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportAgingIndexView(ReportAgingIndexViewModel viewModel)
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
