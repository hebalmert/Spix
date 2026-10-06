using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// Trae las bodegas antes de dejar crear el encabezado del traslado.
public partial class CreateTransferDialogView : UserControl, ISharedModalContent
{
    private readonly CreateTransferDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateTransferDialogView(CreateTransferDialogViewModel viewModel)
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
