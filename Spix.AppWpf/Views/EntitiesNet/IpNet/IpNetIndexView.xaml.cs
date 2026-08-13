using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNet;

// Presenta el indice paginado de direcciones IP destinadas a clientes.
public partial class IpNetIndexView : UserControl
{
    public IpNetIndexView(IpNetIndexViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
