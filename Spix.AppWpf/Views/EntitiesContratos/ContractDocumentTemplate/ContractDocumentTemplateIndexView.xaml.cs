using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// Carga las plantillas por pagina cuando el usuario abre la opcion del menu.
public partial class ContractDocumentTemplateIndexView : UserControl
{
    private readonly ContractDocumentTemplateIndexViewModel _viewModel;
    private bool _isLoaded;

    public ContractDocumentTemplateIndexView(ContractDocumentTemplateIndexViewModel viewModel)
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

        await _viewModel.InitializeAsync();
    }
}
