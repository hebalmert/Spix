using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Mark;

// Presenta una marca nueva sin parametros de contexto.
public partial class CreateMarkDialogView : UserControl, ISharedModalContent
{
    public CreateMarkDialogView(CreateMarkDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters) { }
}
