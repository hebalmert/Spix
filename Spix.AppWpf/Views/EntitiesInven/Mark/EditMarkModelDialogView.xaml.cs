using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Mark;

// Recupera el modelo elegido para que se edite sin perder su marca relacionada.
public partial class EditMarkModelDialogView : UserControl, ISharedModalContent
{
    private readonly EditMarkModelDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditMarkModelDialogView(EditMarkModelDialogViewModel viewModel)
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
