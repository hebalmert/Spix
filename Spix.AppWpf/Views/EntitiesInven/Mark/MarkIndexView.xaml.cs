using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Mark;

// Carga la primera pagina de marcas al abrir el indice desde el menu.
public partial class MarkIndexView : UserControl
{
    private readonly MarkIndexViewModel _viewModel;
    private bool _isLoaded;

    public MarkIndexView(MarkIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, System.Windows.RoutedEventArgs e)
    {
        if (_isLoaded) return;
        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
