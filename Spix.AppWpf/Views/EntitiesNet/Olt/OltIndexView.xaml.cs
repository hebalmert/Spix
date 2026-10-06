using Spix.AppWpf.ViewModels.EntitiesNet.Olt;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Olt;

// Carga la primera pagina de OLT cuando la vista ya fue presentada.
public partial class OltIndexView : UserControl
{
    private readonly OltIndexViewModel _viewModel;
    private bool _loaded;

    public OltIndexView(OltIndexViewModel viewModel)
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
