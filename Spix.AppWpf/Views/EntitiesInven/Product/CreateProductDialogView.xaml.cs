using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Recibe la categoria expandida para crear el producto dentro de ella.
public partial class CreateProductDialogView : UserControl, ISharedModalContent
{
    private readonly CreateProductDialogViewModel _viewModel;
    private Guid _productCategoryId;
    private bool _isLoaded;

    public CreateProductDialogView(CreateProductDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("ProductCategoryId", out var value) == true && value is Guid productCategoryId)
        {
            _productCategoryId = productCategoryId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _productCategoryId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetProductCategory(_productCategoryId);
        await _viewModel.InitializeAsync();
    }
}
