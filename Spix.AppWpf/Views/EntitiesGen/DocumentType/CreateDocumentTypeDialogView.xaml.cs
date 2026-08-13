using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.DocumentType;

// Contiene el formulario de creacion y devuelve el mismo resultado Ok o Cancel del ModalService.
public partial class CreateDocumentTypeDialogView : UserControl, ISharedModalContent
{
    public CreateDocumentTypeDialogView(CreateDocumentTypeDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
