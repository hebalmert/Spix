using Spix.AppWpf.ViewModels.EntitiesInven.Supplier;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Supplier;

// Carga proveedores por pagina cuando el usuario abre la opcion del menu.
public partial class SupplierIndexView : UserControl
{
    private readonly SupplierIndexViewModel _viewModel;
    private bool _isLoaded;

    public SupplierIndexView(SupplierIndexViewModel viewModel)
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
