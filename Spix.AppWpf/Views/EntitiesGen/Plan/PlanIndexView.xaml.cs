using Spix.AppWpf.ViewModels.EntitiesGen.Plan;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Plan;

// Muestra el indice paginado de categorias de planes.
public partial class PlanIndexView : UserControl
{
    private readonly PlanIndexViewModel _viewModel;
    private bool _isLoaded;

    public PlanIndexView(PlanIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadView;
    }

    // Solicita registros solo la primera vez que la vista entra al contenedor principal.
    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
