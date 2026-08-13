using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Serial;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Serial;

// Carga el serial solicitado antes de permitir editar su estado o comentario.
public partial class EditSerialDialogView : UserControl, ISharedModalContent
{
    private readonly EditSerialDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditSerialDialogView(EditSerialDialogViewModel viewModel)
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
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
