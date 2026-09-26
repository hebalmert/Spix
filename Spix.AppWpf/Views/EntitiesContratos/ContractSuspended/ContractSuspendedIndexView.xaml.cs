using Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspended;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractSuspended;

// Contratos suspendidos. Al abrir se pide el listado con los totales del filtro.
public partial class ContractSuspendedIndexView : UserControl
{
    private readonly ContractSuspendedIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractSuspendedIndexView(ContractSuspendedIndexViewModel viewModel)
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
