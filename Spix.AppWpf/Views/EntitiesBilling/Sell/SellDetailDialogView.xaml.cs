using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesBilling.Sell;
using System.Windows.Controls;
using SellEntity = Spix.Domain.EntitiesBilling.Sell;

namespace Spix.AppWpf.Views.EntitiesBilling.Sell;

// El detalle de la factura. Recibe la factura que ya trajo el listado: no pide nada.
public partial class SellDetailDialogView : UserControl, ISharedModalContent
{
    private readonly SellDetailDialogViewModel _viewModel;

    public SellDetailDialogView(SellDetailDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null ||
            !parameters.TryGetValue("Sell", out var valor) ||
            valor is not SellEntity sell)
        {
            return;
        }

        _viewModel.Initialize(sell);
    }
}
