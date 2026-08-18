using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesOper.Client;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesOper.Client;

// Carga el tipo de documento antes de presentar un cliente nuevo.
public partial class CreateClientDialogView : UserControl, ISharedModalContent
{
    private readonly CreateClientDialogViewModel _viewModel;
    private bool _loaded;

    public CreateClientDialogView(CreateClientDialogViewModel viewModel)
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
        await _viewModel.InitializeForCreateAsync();
    }
}
