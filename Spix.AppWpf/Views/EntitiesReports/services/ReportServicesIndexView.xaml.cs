using Spix.AppWpf.ViewModels.EntitiesReports.services;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.services;

// Servicios del periodo. Al abrir se busca el mes en curso, igual que la pantalla web.
public partial class ReportServicesIndexView : UserControl
{
    private readonly ReportServicesIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportServicesIndexView(ReportServicesIndexViewModel viewModel)
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
