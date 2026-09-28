using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Server;

public partial class ServerDetailDialogView : UserControl, ISharedModalContent
{
    private readonly ServerDetailDialogViewModel _viewModel;
    private Guid _serverId;
    private string? _serverIp;
    private bool _loaded;

    public ServerDetailDialogView(ServerDetailDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters == null) return;
        if (parameters.TryGetValue("Id", out var id) && id is Guid serverId) _serverId = serverId;
        if (parameters.TryGetValue("Ip", out var ip)) _serverIp = ip as string;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded) return;
        _loaded = true;
        await _viewModel.InitializeAsync(_serverId, _serverIp);
    }
}
