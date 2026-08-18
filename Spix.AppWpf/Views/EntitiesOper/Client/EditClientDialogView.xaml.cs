using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesOper.Client;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesOper.Client;

// Recupera el cliente individual antes de permitir modificar sus datos.
public partial class EditClientDialogView : UserControl, ISharedModalContent
{
    private readonly EditClientDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditClientDialogView(EditClientDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Id", out object? value) && value is Guid id)
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
        await _viewModel.InitializeForEditAsync(_id);
    }
}
