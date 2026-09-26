using Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Contractor;

// Carga la primera pagina de contratistas cuando la vista se presenta en el contenedor central.
public partial class ContractorIndexView : UserControl
{
    private readonly ContractorIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractorIndexView(ContractorIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
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
