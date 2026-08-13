using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Carga la linea existente junto con la categoria y producto que ya tiene seleccionados.
public partial class EditPurchaseDetailDialogView : UserControl, ISharedModalContent
{
    private readonly EditPurchaseDetailDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditPurchaseDetailDialogView(EditPurchaseDetailDialogViewModel viewModel)
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
