using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Inicializa los combos necesarios para crear un nodo de acceso.
public partial class CreateNodeDialogView : UserControl, ISharedModalContent
{
    private readonly CreateNodeDialogViewModel _viewModel;
    private bool _loaded;

    public CreateNodeDialogView(CreateNodeDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
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
