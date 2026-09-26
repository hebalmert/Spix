using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// Deja listo el combo de tipo antes de presentar una plantilla nueva.
public partial class CreateContractDocumentTemplateDialogView : UserControl, ISharedModalContent
{
    private readonly CreateContractDocumentTemplateDialogViewModel _viewModel;
    private bool _isLoaded;

    public CreateContractDocumentTemplateDialogView(CreateContractDocumentTemplateDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }

    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;

        await _viewModel.InitializeAsync();
    }
}
