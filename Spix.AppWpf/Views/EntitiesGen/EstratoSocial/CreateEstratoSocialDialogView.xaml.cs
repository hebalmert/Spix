using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.EstratoSocial;

// Contiene la creacion de Estrato Social dentro del host modal compartido.
public partial class CreateEstratoSocialDialogView : UserControl, ISharedModalContent
{
    public CreateEstratoSocialDialogView(CreateEstratoSocialDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }
}
