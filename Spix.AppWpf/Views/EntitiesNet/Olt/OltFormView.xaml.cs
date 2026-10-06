using Spix.AppWpf.ViewModels.EntitiesNet.Olt;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Olt;

// Mantiene los selects dependientes y el campo de clave sincronizados con el formulario.
public partial class OltFormView : UserControl
{
    private bool _isSynchronizingPassword;

    public OltFormView()
    {
        InitializeComponent();
    }

    //IsInitializing evita que el evento que dispara el combo al marcar el valor que YA traia
    //borre el hijo cuando se abre el formulario de edicion.
    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is OltFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }

    private async void CitySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is OltFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is City city)
        {
            await viewModel.ChangeCityAsync(city.CityId);
        }
    }

    private async void MarkSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is OltFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is Mark mark)
        {
            await viewModel.ChangeMarkAsync(mark.MarkId);
        }
    }

    private async void CoordinatesLostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is OltFormViewModel viewModel && sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await viewModel.UpdateCoordinatesAsync(textBox.Text);
        }
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSynchronizingPassword || DataContext is not OltFormViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        viewModel.Entity.Clave = passwordBox.Password;
    }

    private void TogglePasswordClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OltFormViewModel viewModel)
        {
            return;
        }

        _isSynchronizingPassword = true;
        PasswordBox.Password = viewModel.Entity.Clave ?? string.Empty;
        PasswordTextBox.Text = viewModel.Entity.Clave ?? string.Empty;
        _isSynchronizingPassword = false;
    }
}
