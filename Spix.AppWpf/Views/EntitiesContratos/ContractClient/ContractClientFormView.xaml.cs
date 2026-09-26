using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using Spix.Domain.Entities;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractClient;

// Recarga las ciudades al elegir otro estado, y las zonas al elegir otra ciudad.
//
// Va con SelectionChanged y el enlace de OneWay, no con TwoWay: en TwoWay el enlace
// escribe el valor ANTES de que llegue el evento, la guarda cree que no hubo cambio y la
// cascada nunca se carga. Es la misma trampa de Zonas y Proveedores.
public partial class ContractClientFormView : UserControl
{
    public ContractClientFormView()
    {
        InitializeComponent();
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ContractClientFormViewModel viewModel &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }

    private async void CitySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ContractClientFormViewModel viewModel &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is City city)
        {
            await viewModel.ChangeCityAsync(city.CityId);
        }
    }
}
