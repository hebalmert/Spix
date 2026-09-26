using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// Carga los usuarios por pagina cuando se abre la opcion del menu.
public partial class UsuarioIndexView : UserControl
{
    private readonly UsuarioIndexViewModel _viewModel;
    private bool _isLoaded;

    public UsuarioIndexView(UsuarioIndexViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarPantalla;
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync();
    }
}
