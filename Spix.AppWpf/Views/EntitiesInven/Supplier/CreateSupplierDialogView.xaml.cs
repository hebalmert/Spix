using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Supplier;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Supplier;

// Carga los combos antes de presentar un proveedor nuevo.
public partial class CreateSupplierDialogView : UserControl, ISharedModalContent
{
    private readonly CreateSupplierDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateSupplierDialogView(CreateSupplierDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
