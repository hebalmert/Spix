using Spix.AppWpf.ViewModels.EntitiesGen.Zone;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Zone;

// Recarga las ciudades disponibles al cambiar el estado, igual que el formulario de Blazor.
public partial class ZoneFormView : UserControl
{
    public ZoneFormView()
    {
        InitializeComponent();
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ZoneFormViewModel viewModel &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is Spix.Domain.Entities.State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }
}
