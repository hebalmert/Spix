using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// El cascaron del modal para agregar un rol al usuario que abrio la pantalla.
public partial class CreateUsuarioRoleDialogView : UserControl, ISharedModalContent
{
    private readonly CreateUsuarioRoleDialogViewModel _viewModel;
    private Guid _usuarioId;
    private bool _isLoaded;

    public CreateUsuarioRoleDialogView(CreateUsuarioRoleDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarModal;
    }

    // Solo se guarda el id: la lista de roles se pide en Loaded, no aqui
    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id)
        {
            _usuarioId = id;
        }
    }

    private async void CargarModal(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _usuarioId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetUsuario(_usuarioId);
        await _viewModel.InitializeAsync();
    }
}
