using Spix.AppWpf.ViewModels.EntitiesInven.Supplier;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Supplier;

// Recarga las ciudades al seleccionar otro estado.
public partial class SupplierFormView : UserControl
{
    public SupplierFormView()
    {
        InitializeComponent();
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is SupplierFormViewModel viewModel && e.AddedItems.Count > 0 && e.AddedItems[0] is Spix.Domain.Entities.State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }
}
