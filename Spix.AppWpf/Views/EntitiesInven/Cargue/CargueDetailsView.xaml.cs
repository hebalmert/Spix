using Spix.AppWpf.ViewModels.EntitiesInven.Cargue;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Cargue;

// Presenta la recepcion seleccionada y permite volver al indice de cargues.
public partial class CargueDetailsView : UserControl
{
    private readonly CargueDetailsViewModel _viewModel;
    private Guid _cargueId;
    private bool _isLoaded;

    public event EventHandler? BackRequested;

    public CargueDetailsView(CargueDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.BackRequested += ReturnToIndex;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    public void LoadCargue(Guid cargueId)
    {
        _cargueId = cargueId;
    }

    private void ReturnToIndex(object? sender, EventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded || _cargueId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_cargueId);
    }
}
