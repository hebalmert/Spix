using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesPayment.CxCBill;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.CxCBill;

// Anular la nota. No pide nada al servidor: solo recibe el id y el motivo.
public partial class CxCBillCancelDialogView : UserControl, ISharedModalContent
{
    private readonly CxCBillCancelDialogViewModel _viewModel;

    public CxCBillCancelDialogView(CxCBillCancelDialogViewModel viewModel)
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
