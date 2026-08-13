using Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;

// Presenta la configuracion paginada de control MikroTik de la corporacion.
public partial class ConnectionMikrotikControlIndexView : UserControl
{
    public ConnectionMikrotikControlIndexView(ConnectionMikrotikControlIndexViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
