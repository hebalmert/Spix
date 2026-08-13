using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.IpNet;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.IpNet;

// Recupera la IP seleccionada antes de mostrar el formulario de edicion.
public partial class EditIpNetDialogView : UserControl, ISharedModalContent
{
    private readonly EditIpNetDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditIpNetDialogView(EditIpNetDialogViewModel viewModel)
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
