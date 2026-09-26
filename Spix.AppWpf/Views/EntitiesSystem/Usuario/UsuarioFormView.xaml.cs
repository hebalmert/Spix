using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// El formulario que comparten Crear y Editar usuario.
public partial class UsuarioFormView : UserControl
{
    public UsuarioFormView()
    {
        InitializeComponent();
    }

    // La foto llega en el mismo Base64 venga del disco o de la camara
    private void PhotoSelected(object? sender, string base64)
    {
        if (DataContext is UsuarioFormViewModel viewModel)
        {
            viewModel.SetPhoto(base64);
        }
    }
}
