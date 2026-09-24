using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Tax;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Tax;

// Carga el impuesto recibido antes de permitir que el usuario lo modifique.
public partial class EditTaxDialogView : UserControl, ISharedModalContent
{
    private readonly EditTaxDialogViewModel _viewModel;
    private Guid _taxId;
    private bool _isLoaded;

    public EditTaxDialogView(EditTaxDialogViewModel viewModel)
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
            id is not Guid taxId)
        {
            return;
        }

        _taxId = taxId;
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _taxId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadForEditAsync(_taxId);
    }
}
