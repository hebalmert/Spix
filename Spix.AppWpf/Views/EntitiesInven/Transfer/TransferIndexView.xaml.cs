using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// El listado de traslados. Le avisa a la ventana principal cuando hay que abrir el
// detalle, para no amarrar el ViewModel a la navegacion.
public partial class TransferIndexView : UserControl
{
    private readonly TransferIndexViewModel _viewModel;
    private bool _isLoaded;

    public event EventHandler<Guid>? DetailsRequested;

    public TransferIndexView(TransferIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.DetailsRequested += RequestDetails;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private void RequestDetails(object? sender, Guid transferId)
    {
        DetailsRequested?.Invoke(this, transferId);
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
