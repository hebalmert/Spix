using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.ContractorCxC;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractorCxC;

// Anular la cuenta. No pide nada al servidor: solo recibe el id y el motivo.
public partial class ContractorCxCCancelDialogView : UserControl, ISharedModalContent
{
    private readonly ContractorCxCCancelDialogViewModel _viewModel;

    public ContractorCxCCancelDialogView(ContractorCxCCancelDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null &&
            parameters.TryGetValue("Id", out var valor) &&
            valor is Guid id)
        {
            _viewModel.SetId(id);
        }
    }
}
