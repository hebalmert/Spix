using Spix.AppWpf.ViewModels.EntitiesReports.cutoff;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.cutoff;

// Efectividad del corte. Al abrir ya trae el mes en curso, igual que la pantalla web.
public partial class ReportCutOffIndexView : UserControl
{
    private readonly ReportCutOffIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportCutOffIndexView(ReportCutOffIndexViewModel viewModel)
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
