using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNet;

// Presenta el formulario que crea un rango de IPs de clientes.
public partial class CreateIpNetPoolDialogView : UserControl, ISharedModalContent
{
    public CreateIpNetPoolDialogView(CreateIpNetPoolDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
