using Spix.AppWpf.ViewModels.EntitiesOper.Client;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesOper.Client;

// Carga la primera pagina de clientes cuando la vista se presenta en el contenedor central.
public partial class ClientIndexView : UserControl
{
    private readonly ClientIndexViewModel _viewModel;
    private bool _loaded;

    public ClientIndexView(ClientIndexViewModel viewModel)
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
