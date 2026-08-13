using Spix.AppWpf.ViewModels.EntitiesInven.Storage;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Storage;

// Carga bodegas de forma paginada al entrar al modulo.
public partial class ProductStorageIndexView : UserControl
{
    private readonly ProductStorageIndexViewModel _viewModel;
    private bool _isLoaded;

    public ProductStorageIndexView(ProductStorageIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
