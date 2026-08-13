using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Presenta el detalle de una compra y conserva su retorno al indice principal.
public partial class PurchaseDetailsView : UserControl
{
    private readonly PurchaseDetailsViewModel _viewModel;
    private Guid _purchaseId;
    private bool _isLoaded;

    public event EventHandler? BackRequested;

    public PurchaseDetailsView(PurchaseDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.BackRequested += ReturnToIndex;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    // Define la compra elegida por el indice antes de insertar esta vista en el contenido central.
    public void LoadPurchase(Guid purchaseId)
    {
        _purchaseId = purchaseId;
    }

    private void ReturnToIndex(object? sender, EventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded || _purchaseId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_purchaseId);
    }
}
