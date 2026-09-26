using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// Trae la plantilla elegida para editar sus datos. Los campos del PDF no viajan en este PUT.
public partial class EditContractDocumentTemplateDialogView : UserControl, ISharedModalContent
{
    private readonly EditContractDocumentTemplateDialogViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;

    public EditContractDocumentTemplateDialogView(EditContractDocumentTemplateDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var valor) == true && valor is Guid id)
        {
            _id = id;
        }
    }

    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;

        await _viewModel.InitializeAsync();
        await _viewModel.LoadForEditAsync(_id);
    }
}
