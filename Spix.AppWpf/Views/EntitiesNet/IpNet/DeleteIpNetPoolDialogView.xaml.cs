using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNet;

// Presenta el formulario que elimina un rango de IPs de clientes no asignadas.
public partial class DeleteIpNetPoolDialogView : UserControl, ISharedModalContent
{
    public DeleteIpNetPoolDialogView(DeleteIpNetPoolDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
