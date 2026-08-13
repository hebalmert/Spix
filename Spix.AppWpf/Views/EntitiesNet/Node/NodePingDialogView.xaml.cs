using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Recibe el nodo seleccionado y ejecuta el ping cuando el modal termina de cargarse.
public partial class NodePingDialogView : UserControl, ISharedModalContent
{
    private readonly NodePingDialogViewModel _viewModel;
    private bool _loaded;

    public NodePingDialogView(NodePingDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Host", out var hostValue) != true)
        {
            return;
        }

        parameters.TryGetValue("NodeName", out var nodeNameValue);
        _viewModel.SetHost(hostValue?.ToString() ?? string.Empty, nodeNameValue?.ToString());
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
