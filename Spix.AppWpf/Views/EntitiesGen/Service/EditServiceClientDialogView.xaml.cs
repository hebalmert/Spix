using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Service;

// Carga un servicio existente antes de exponerlo en el formulario comun.
public partial class EditServiceClientDialogView : UserControl, ISharedModalContent
{
    private readonly EditServiceClientDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditServiceClientDialogView(EditServiceClientDialogViewModel viewModel)
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
        await _viewModel.InitializeAsync();
        await _viewModel.LoadAsync(_id);
    }
}
