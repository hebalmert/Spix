using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNet;

// Contiene el formulario de creacion individual de una IP de cliente.
public partial class CreateIpNetDialogView : UserControl, ISharedModalContent
{
    public CreateIpNetDialogView(CreateIpNetDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
