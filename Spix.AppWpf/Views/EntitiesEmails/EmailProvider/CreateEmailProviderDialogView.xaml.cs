using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesEmails.EmailProvider;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesEmails.EmailProvider;

// Contiene el formulario de creacion y devuelve el mismo resultado Ok o Cancel del ModalService.
public partial class CreateEmailProviderDialogView : UserControl, ISharedModalContent
{
    public CreateEmailProviderDialogView(CreateEmailProviderDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
