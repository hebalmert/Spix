using Spix.AppWpf.ViewModels.EntitiesContratos.ContractExempt;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractExempt;

// Exoneracion fija. Al abrir se pide el listado con sus totales.
public partial class ContractExemptIndexView : UserControl
{
    private readonly ContractExemptIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractExemptIndexView(ContractExemptIndexViewModel viewModel)
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
