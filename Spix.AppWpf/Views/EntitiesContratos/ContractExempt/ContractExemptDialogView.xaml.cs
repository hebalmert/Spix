using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractExempt;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractExempt;

// Exonerar un contrato. No recibe parametros: el contrato se busca aqui adentro.
public partial class ContractExemptDialogView : UserControl, ISharedModalContent
{
    public ContractExemptDialogView(ContractExemptDialogViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
