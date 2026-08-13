using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Mark;

// Recupera la marca elegida antes de mostrar el formulario compartido.
public partial class EditMarkDialogView : UserControl, ISharedModalContent
{
    private readonly EditMarkDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditMarkDialogView(EditMarkDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var value) == true && value is Guid id) _id = id;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty) return;
        _isLoaded = true;
        await _viewModel.LoadAsync(_id);
    }
}
