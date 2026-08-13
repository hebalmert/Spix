using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Cargue;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Cargue;

// Recupera una MAC existente antes de habilitar su actualizacion dentro del cargue pendiente.
public partial class EditCargueDetailDialogView : UserControl, ISharedModalContent
{
    private readonly EditCargueDetailDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditCargueDetailDialogView(EditCargueDetailDialogViewModel viewModel)
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
