using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Carga el indice de compras y expone su navegacion de detalle a la ventana principal.
public partial class PurchaseIndexView : UserControl
{
    private readonly PurchaseIndexViewModel _viewModel;
    private bool _isLoaded;

    public event EventHandler<Guid>? DetailsRequested;

    public PurchaseIndexView(PurchaseIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.DetailsRequested += RequestDetails;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    // Propaga el identificador sin acoplar el ViewModel a la navegacion visual.
    private void RequestDetails(object? sender, Guid purchaseId)
    {
        DetailsRequested?.Invoke(this, purchaseId);
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
