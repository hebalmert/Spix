using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractSuspended;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractSuspended;

// Suspender un contrato. No recibe parametros: el contrato se busca aqui adentro.
public partial class ContractSuspendedDialogView : UserControl, ISharedModalContent
{
    public ContractSuspendedDialogView(ContractSuspendedDialogViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
