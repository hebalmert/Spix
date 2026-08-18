using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Server;

// Expone el listado WPF de servidores usando su ViewModel inyectado.
public partial class ServerIndexView : UserControl
{
    private readonly ServerIndexViewModel _viewModel;
    private bool _loaded;

    public ServerIndexView(ServerIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

    // Carga la primera pagina de servidores cuando la vista ya fue presentada.
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
