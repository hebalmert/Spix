using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Zone;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Zone;

// Carga los estados antes de crear una zona.
public partial class CreateZoneDialogView : UserControl, ISharedModalContent
{
    private readonly CreateZoneDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateZoneDialogView(CreateZoneDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeAsync();
    }
}
