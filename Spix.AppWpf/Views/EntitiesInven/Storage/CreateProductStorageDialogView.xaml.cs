using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Storage;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Storage;

// Carga los estados antes de crear una bodega.
public partial class CreateProductStorageDialogView : UserControl, ISharedModalContent
{
    private readonly CreateProductStorageDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateProductStorageDialogView(CreateProductStorageDialogViewModel viewModel)
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
