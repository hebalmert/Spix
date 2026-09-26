using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Technitian;

// Carga el tipo de documento antes de presentar un tecnico nuevo.
public partial class CreateTechnitianDialogView : UserControl, ISharedModalContent
{
    private readonly CreateTechnitianDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateTechnitianDialogView(CreateTechnitianDialogViewModel viewModel)
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
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.InitializeForCreateAsync();
    }
}
