using Spix.AppWpf.SharedComponents;
using AuditTransferRow = Spix.AppWpf.ViewModels.EntitiesInven.Transfer.AuditTransferRow;
using System.Windows.Controls;
using TransferEntity = Spix.Domain.EntitiesInven.Transfer;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// El historial de un traslado. El registro llega completo desde el index, asi que esta
// ventana solo pinta: no hay peticion al servidor ni spinner que mostrar.
public partial class AuditTransferDialogView : UserControl, ISharedModalContent
{
    public AuditTransferDialogView()
    {
        InitializeComponent();
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Transfer", out var value) == true && value is TransferEntity transfer)
        {
            DataContext = new AuditTransferRow(transfer);
        }
    }
}
