using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Recibe la compra seleccionada para agregarle una nueva linea de producto.
public partial class CreatePurchaseDetailDialogView : UserControl, ISharedModalContent
{
    private readonly CreatePurchaseDetailDialogViewModel _viewModel;
    private Guid _purchaseId;
    private bool _isLoaded;

    public CreatePurchaseDetailDialogView(CreatePurchaseDetailDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("PurchaseId", out var value) == true && value is Guid purchaseId)
        {
            _purchaseId = purchaseId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _purchaseId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetPurchaseId(_purchaseId);
        await _viewModel.InitializeAsync();
    }
}
