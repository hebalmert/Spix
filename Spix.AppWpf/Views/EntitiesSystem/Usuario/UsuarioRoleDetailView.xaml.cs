using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// Los roles de un usuario. Se abre desde el listado, con su id.
public partial class UsuarioRoleDetailView : UserControl
{
    private readonly UsuarioRoleDetailViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    // La pantalla no sabe volver: lo decide quien la abrio
    public event EventHandler? BackRequested;

    public UsuarioRoleDetailView(UsuarioRoleDetailViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.BackRequested += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);

        Loaded += CargarPantalla;
    }

    // Se le dice de que usuario es ANTES de mostrarla
    public void Prepare(Guid id)
    {
        _id = id;
    }

    private async void CargarPantalla(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
