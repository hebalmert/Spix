using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Server;

// Ejecuta el ping local cuando el usuario abre el diagnostico de un servidor.
public partial class ServerPingDialogView : UserControl, ISharedModalContent
{
    private readonly ServerPingDialogViewModel _viewModel;
    private bool _loaded;

    public ServerPingDialogView(ServerPingDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        object? hostValue = null;
        object? serverNameValue = null;

        parameters?.TryGetValue("Host", out hostValue);
        parameters?.TryGetValue("ServerName", out serverNameValue);
        _viewModel.SetHost(hostValue?.ToString() ?? string.Empty, serverNameValue?.ToString());
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync();
    }
}
