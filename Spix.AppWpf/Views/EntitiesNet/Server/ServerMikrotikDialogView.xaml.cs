using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Server;

// Recibe el id del servidor y ejecuta su prueba MikroTik desde Windows al abrir el modal.
public partial class ServerMikrotikDialogView : UserControl, ISharedModalContent
{
    private readonly ServerMikrotikDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public ServerMikrotikDialogView(ServerMikrotikDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out object? value) == true && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id);
    }
}
