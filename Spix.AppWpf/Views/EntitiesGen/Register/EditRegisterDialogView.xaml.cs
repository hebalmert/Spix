using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Register;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Register;

// Carga los consecutivos recibidos antes de permitir que el usuario los modifique.
public partial class EditRegisterDialogView : UserControl, ISharedModalContent
{
    private readonly EditRegisterDialogViewModel _viewModel;
    private Guid _registerId;
    private bool _isLoaded;

    public EditRegisterDialogView(EditRegisterDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("Id", out var id) ||
            id is not Guid registerId)
        {
            return;
        }

        _registerId = registerId;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _registerId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadForEditAsync(_registerId);
    }
}
