using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.DocumentType;

// Carga el registro recibido antes de permitir que el usuario lo modifique.
public partial class EditDocumentTypeDialogView : UserControl, ISharedModalContent
{
    private readonly EditDocumentTypeDialogViewModel _viewModel;
    private Guid _documentTypeId;
    private bool _isLoaded;

    public EditDocumentTypeDialogView(EditDocumentTypeDialogViewModel viewModel)
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
            id is not Guid documentTypeId)
        {
            return;
        }

        _documentTypeId = documentTypeId;
    }

    // Activa el spinner del modal mientras se realiza el GET individual del registro.
    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _documentTypeId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        await _viewModel.LoadAsync(_documentTypeId);
    }
}
