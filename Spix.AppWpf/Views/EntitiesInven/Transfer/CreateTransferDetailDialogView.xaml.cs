using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// Recibe el traslado al que se le agrega la linea. El id del traslado se pone ANTES de
// cargar: sin el no se puede preguntar cuanto hay en la bodega de origen.
public partial class CreateTransferDetailDialogView : UserControl, ISharedModalContent
{
    private readonly CreateTransferDetailDialogViewModel _viewModel;
    private Guid _transferId;
    private bool _isLoaded;

    public CreateTransferDetailDialogView(CreateTransferDetailDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("TransferId", out var value) == true && value is Guid transferId)
        {
            _transferId = transferId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _transferId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetTransferId(_transferId);
        await _viewModel.InitializeAsync();
    }
}
