using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNetwork;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNetwork;

// Carga la direccion IP de red elegida antes de abrir la edicion.
public partial class EditIpNetworkDialogView : UserControl, ISharedModalContent
{
    private readonly EditIpNetworkDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditIpNetworkDialogView(EditIpNetworkDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded || _id == Guid.Empty)
        {
            return;
        }

        _loaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
