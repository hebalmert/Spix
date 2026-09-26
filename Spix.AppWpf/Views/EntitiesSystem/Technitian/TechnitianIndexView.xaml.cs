using Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Technitian;

// Carga la primera pagina de tecnicos cuando la vista se presenta en el contenedor central.
public partial class TechnitianIndexView : UserControl
{
    private readonly TechnitianIndexViewModel _viewModel;
    private bool _isLoaded;

    public TechnitianIndexView(TechnitianIndexViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadView;
    }

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
