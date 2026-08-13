using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Carga la primera pagina de nodos cuando la vista ya fue presentada.
public partial class NodeIndexView : UserControl
{
    private readonly NodeIndexViewModel _viewModel;
    private bool _loaded;

    public NodeIndexView(NodeIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    private async void LoadView(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.LoadAsync();
    }
}
