using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesMK.QueueType;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesMK.QueueType;

// Hospeda el formulario para crear un Queue Type.
public partial class CreateQueueTypeDialogView : UserControl, ISharedModalContent
{
    public CreateQueueTypeDialogView(CreateQueueTypeDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
