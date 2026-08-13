using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Carga los proveedores y bodegas antes de crear el encabezado de compra.
public partial class CreatePurchaseDialogView : UserControl, ISharedModalContent
{
    private readonly CreatePurchaseDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreatePurchaseDialogView(CreatePurchaseDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
