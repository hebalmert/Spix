using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesMK.ConnectionMikrotikControl;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesMK.ConnectionMikrotikControl;

// Hospeda el formulario para crear el control MikroTik de la corporacion.
public partial class CreateConnectionMikrotikControlDialogView : UserControl, ISharedModalContent
{
    public CreateConnectionMikrotikControlDialogView(CreateConnectionMikrotikControlDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
    }
}
