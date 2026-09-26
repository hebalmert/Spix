using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Technitian;

// Carga el combo y el tecnico seleccionado antes de permitir modificar sus datos.
public partial class EditTechnitianDialogView : UserControl, ISharedModalContent
{
    private readonly EditTechnitianDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditTechnitianDialogView(EditTechnitianDialogViewModel viewModel)
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
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadForEditAsync(_id);
    }
}
