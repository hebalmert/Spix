using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;

// Carga el control MikroTik seleccionado antes de editarlo.
public partial class EditConnectionMikrotikControlDialogView : UserControl, ISharedModalContent
{
    private readonly EditConnectionMikrotikControlDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditConnectionMikrotikControlDialogView(EditConnectionMikrotikControlDialogViewModel viewModel)
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
