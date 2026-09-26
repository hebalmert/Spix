using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesBilling.BillingNote;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesBilling.BillingNote;

// El detalle de la nota general y su lanzamiento. Trae los meses y la nota al abrir.
public partial class BillingNoteDetailDialogView : UserControl, ISharedModalContent
{
    private readonly BillingNoteDetailDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public BillingNoteDetailDialogView(BillingNoteDetailDialogViewModel viewModel)
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
