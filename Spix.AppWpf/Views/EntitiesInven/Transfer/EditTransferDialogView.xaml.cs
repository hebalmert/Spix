using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// El encabezado se carga DESPUES de las bodegas, o los dos selects llegan sin su lista
// y el valor guardado no se puede seleccionar.
public partial class EditTransferDialogView : UserControl, ISharedModalContent
{
    private readonly EditTransferDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditTransferDialogView(EditTransferDialogViewModel viewModel)
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
