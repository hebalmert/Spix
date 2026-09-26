using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNote;

// Nueva nota general. Carga el combo de meses antes de presentar el formulario.
public partial class CreateBillingNoteDialogView : UserControl, ISharedModalContent
{
    private readonly CreateBillingNoteDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateBillingNoteDialogView(CreateBillingNoteDialogViewModel viewModel)
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
        await _viewModel.InitializeAsync();
    }
}
