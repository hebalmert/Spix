using Spix.AppWpf.ViewModels.EntitiesReports.churn;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.churn;

// Contratos fuera de servicio. Al abrir se baja el tablero y las zonas: no hay nada que elegir.
public partial class ReportChurnIndexView : UserControl
{
    private readonly ReportChurnIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportChurnIndexView(ReportChurnIndexViewModel viewModel)
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
