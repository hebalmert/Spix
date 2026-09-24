using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Register;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Register;

// Contiene el formulario de creacion y devuelve el mismo resultado Ok o Cancel del ModalService.
public partial class CreateRegisterDialogView : UserControl, ISharedModalContent
{
    public CreateRegisterDialogView(CreateRegisterDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
