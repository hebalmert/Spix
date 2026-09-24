using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Recibe el producto y pide sus existencias al abrirse.
public partial class ProductStockDialogView : UserControl, ISharedModalContent
{
    private readonly ProductStockDialogViewModel _viewModel;
    private Guid _productId;
    private bool _isLoaded;

    public ProductStockDialogView(ProductStockDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("Id", out var id) ||
            id is not Guid productId)
        {
            return;
        }

        _productId = productId;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _productId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_productId);
    }
}
