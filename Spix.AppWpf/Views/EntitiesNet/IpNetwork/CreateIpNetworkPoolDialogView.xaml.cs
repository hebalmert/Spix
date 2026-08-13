using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNetwork;

// Presenta la creacion de un rango completo de IPs de red.
public partial class CreateIpNetworkPoolDialogView : UserControl, ISharedModalContent
{
    public CreateIpNetworkPoolDialogView(CreateIpNetworkPoolDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
