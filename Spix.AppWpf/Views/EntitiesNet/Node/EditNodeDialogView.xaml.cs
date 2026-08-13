using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Recupera el nodo elegido antes de cargar todos sus selects dependientes.
public partial class EditNodeDialogView : UserControl, ISharedModalContent
{
    private readonly EditNodeDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditNodeDialogView(EditNodeDialogViewModel viewModel)
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
        await _viewModel.LoadForEditAsync(_id);
    }
}
