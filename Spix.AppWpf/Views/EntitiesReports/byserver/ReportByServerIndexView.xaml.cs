using Spix.AppWpf.ViewModels.EntitiesReports.byserver;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.byserver;

// Contratos por servidor. Al abrir solo bajan las dos listas: lo demas espera a que se elija
// un servidor.
public partial class ReportByServerIndexView : UserControl
{
    private readonly ReportByServerIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportByServerIndexView(ReportByServerIndexViewModel viewModel)
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
