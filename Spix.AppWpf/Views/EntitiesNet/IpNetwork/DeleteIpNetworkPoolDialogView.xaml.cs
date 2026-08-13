using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNetwork;

// Presenta la eliminacion de un rango de IPs de red no asignadas.
public partial class DeleteIpNetworkPoolDialogView : UserControl, ISharedModalContent
{
    public DeleteIpNetworkPoolDialogView(DeleteIpNetworkPoolDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
