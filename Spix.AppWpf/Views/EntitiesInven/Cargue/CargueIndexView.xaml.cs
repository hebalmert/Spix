using Spix.AppWpf.ViewModels.EntitiesInven.Cargue;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Cargue;

// Carga los cargues de la corporacion y expone su navegacion de detalle.
public partial class CargueIndexView : UserControl
{
    private readonly CargueIndexViewModel _viewModel;
    private bool _isLoaded;

    public event EventHandler<Guid>? DetailsRequested;

    public CargueIndexView(CargueIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.DetailsRequested += RequestDetails;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private void RequestDetails(object? sender, Guid cargueId)
    {
        DetailsRequested?.Invoke(this, cargueId);
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
