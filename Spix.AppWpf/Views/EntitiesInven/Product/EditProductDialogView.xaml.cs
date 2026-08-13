using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Carga los combos y el producto existente antes de permitir su actualizacion.
public partial class EditProductDialogView : UserControl, ISharedModalContent
{
    private readonly EditProductDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditProductDialogView(EditProductDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
        await _viewModel.LoadForEditAsync(_id);
    }
}
