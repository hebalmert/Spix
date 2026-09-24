using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Tax;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Tax;

// Contiene el formulario de creacion y devuelve el mismo resultado Ok o Cancel del ModalService.
public partial class CreateTaxDialogView : UserControl, ISharedModalContent
{
    public CreateTaxDialogView(CreateTaxDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
