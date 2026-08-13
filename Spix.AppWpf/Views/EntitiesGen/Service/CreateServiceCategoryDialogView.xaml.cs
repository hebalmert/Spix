using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Service;

// Contiene la creacion de Categoria Servicio dentro del host modal compartido.
public partial class CreateServiceCategoryDialogView : UserControl, ISharedModalContent
{
    public CreateServiceCategoryDialogView(CreateServiceCategoryDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }
}
