using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Recupera el encabezado de compra luego de cargar los selects que lo componen.
public partial class EditPurchaseDialogView : UserControl, ISharedModalContent
{
    private readonly EditPurchaseDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditPurchaseDialogView(EditPurchaseDialogViewModel viewModel)
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
