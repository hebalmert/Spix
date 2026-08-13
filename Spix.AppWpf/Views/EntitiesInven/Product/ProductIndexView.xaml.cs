using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Inicializa el indice de productos y carga solamente su primera pagina.
public partial class ProductIndexView : UserControl
{
    private readonly ProductIndexViewModel _viewModel;
    private bool _isLoaded;

    public ProductIndexView(ProductIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
