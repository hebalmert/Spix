using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// Los equipos que viajaron en una linea del traslado. Se consulta al abrir, igual que los
// demas dialogos que solo leen.
public partial class TransferDetailSerialsDialogView : UserControl, ISharedModalContent
{
    private readonly TransferDetailSerialsViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public TransferDetailSerialsDialogView(TransferDetailSerialsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("TransferDetailsId", out var valor) == true && valor is Guid id)
        {
            _id = id;
        }

        if (parameters?.TryGetValue("ProductName", out var nombre) == true && nombre is string texto)
        {
            _viewModel.ProductName = texto;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
