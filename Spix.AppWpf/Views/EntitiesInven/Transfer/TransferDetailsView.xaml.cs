using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// El detalle de un traslado. Guarda el regreso al listado sin acoplar el ViewModel a la
// ventana principal.
public partial class TransferDetailsView : UserControl
{
    private readonly TransferDetailsViewModel _viewModel;
    private Guid _transferId;
    private bool _isLoaded;

    public event EventHandler? BackRequested;

    public TransferDetailsView(TransferDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.BackRequested += ReturnToIndex;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    // El traslado elegido en el listado, antes de montar la vista
    public void LoadTransfer(Guid transferId)
    {
        _transferId = transferId;
    }

    private void ReturnToIndex(object? sender, EventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded || _transferId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_transferId);
    }
}
