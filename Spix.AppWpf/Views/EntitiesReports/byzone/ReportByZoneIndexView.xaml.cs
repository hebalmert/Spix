using Spix.AppWpf.ViewModels.EntitiesReports.byzone;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.byzone;

// Contratos por zona. Al abrir solo bajan las listas de estados: lo demas espera a que se
// elija una zona.
public partial class ReportByZoneIndexView : UserControl
{
    private readonly ReportByZoneIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportByZoneIndexView(ReportByZoneIndexViewModel viewModel)
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
