using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNetwork;

// Presenta el indice paginado de direcciones IP empleadas por nodos y servidores.
public partial class IpNetworkIndexView : UserControl
{
    public IpNetworkIndexView(IpNetworkIndexViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
