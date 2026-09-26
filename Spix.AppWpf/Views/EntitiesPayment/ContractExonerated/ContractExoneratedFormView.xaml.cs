using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesPayment.ContractExonerated;

// El formulario que comparten el alta y la edicion. No tiene DataContext propio: hereda el
// del modal que lo contiene, igual que el formulario de proveedores.
public partial class ContractExoneratedFormView : UserControl
{
    public ContractExoneratedFormView()
    {
        InitializeComponent();
    }
}
