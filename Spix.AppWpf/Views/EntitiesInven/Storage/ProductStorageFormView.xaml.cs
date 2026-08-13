using Spix.AppWpf.ViewModels.EntitiesInven.Storage;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Storage;

// Recarga las ciudades disponibles para la bodega al cambiar el estado.
public partial class ProductStorageFormView : UserControl
{
    public ProductStorageFormView()
    {
        InitializeComponent();
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ProductStorageFormViewModel viewModel && e.AddedItems.Count > 0 && e.AddedItems[0] is Spix.Domain.Entities.State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }
}
