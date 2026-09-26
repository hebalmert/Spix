using Spix.AppWpf.ViewModels.EntitiesReports.contracts;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesReports.contracts;

// Contratos del periodo. Al abrir ya se muestra el mes en curso, igual que la pantalla web.
public partial class ReportContractsIndexView : UserControl
{
    private readonly ReportContractsIndexViewModel _viewModel;
    private bool _isLoaded;

    public ReportContractsIndexView(ReportContractsIndexViewModel viewModel)
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
