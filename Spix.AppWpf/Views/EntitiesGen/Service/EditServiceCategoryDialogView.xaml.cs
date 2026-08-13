using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Service;

// Carga la categoria de servicios recibida antes de permitir su edicion.
public partial class EditServiceCategoryDialogView : UserControl, ISharedModalContent
{
    private readonly EditServiceCategoryDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditServiceCategoryDialogView(EditServiceCategoryDialogViewModel viewModel)
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
