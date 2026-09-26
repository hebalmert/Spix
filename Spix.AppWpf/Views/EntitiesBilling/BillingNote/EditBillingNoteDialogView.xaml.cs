using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNote;

// Editar la nota general. Trae los meses y la nota antes de presentarla.
public partial class EditBillingNoteDialogView : UserControl, ISharedModalContent
{
    private readonly EditBillingNoteDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditBillingNoteDialogView(EditBillingNoteDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
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
        await _viewModel.InitializeAsync(_id);
    }
}
