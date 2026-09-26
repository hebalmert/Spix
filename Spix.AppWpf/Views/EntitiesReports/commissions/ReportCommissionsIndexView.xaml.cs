using Spix.AppWpf.ViewModels.EntitiesReports.commissions;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.commissions;

// Comisiones de contratistas. Al abrir ya trae el mes en curso, igual que la web.
public partial class ReportCommissionsIndexView : UserControl
{
    private readonly ReportCommissionsIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportCommissionsIndexView(ReportCommissionsIndexViewModel viewModel)
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
