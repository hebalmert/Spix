using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Presenta el formulario de creacion sin parametros adicionales.
public partial class CreateProductCategoryDialogView : UserControl, ISharedModalContent
{
    public CreateProductCategoryDialogView(CreateProductCategoryDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
