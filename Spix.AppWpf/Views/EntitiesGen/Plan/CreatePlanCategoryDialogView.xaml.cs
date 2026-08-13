using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Plan;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Plan;

// Contiene la creacion de Categoria Plan dentro del host modal compartido.
public partial class CreatePlanCategoryDialogView : UserControl, ISharedModalContent
{
    public CreatePlanCategoryDialogView(CreatePlanCategoryDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }
}
