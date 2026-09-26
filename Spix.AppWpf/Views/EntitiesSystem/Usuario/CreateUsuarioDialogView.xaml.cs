using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Usuario;

// El cascaron del modal para crear un usuario: no trae datos, arranca en blanco y activo.
public partial class CreateUsuarioDialogView : UserControl, ISharedModalContent
{
    public CreateUsuarioDialogView(CreateUsuarioDialogViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }
}
