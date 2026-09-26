using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractClient;

// Contratos de los clientes. Al abrir se pide la primera pagina.
public partial class ContractClientIndexView : UserControl
{
    private readonly ContractClientIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractClientIndexView(ContractClientIndexViewModel viewModel)
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
        await _viewModel.LoadAsync();
    }
}
