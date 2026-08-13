using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNetwork;

// Contiene la creacion individual de una direccion IP de red.
public partial class CreateIpNetworkDialogView : UserControl, ISharedModalContent
{
    public CreateIpNetworkDialogView(CreateIpNetworkDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
