using Spix.AppWpf.ViewModels.EntitiesNet.Server;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Server;

// Mantiene los select dependientes y la clave sincronizados con el formulario de servidor.
public partial class ServerFormView : UserControl
{
    private bool _isSynchronizingPassword;

    public ServerFormView()
    {
        InitializeComponent();
    }

    private async void MarkSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ServerFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is Mark mark)
        {
            await viewModel.ChangeMarkAsync(mark.MarkId);
        }
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ServerFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }

    private async void CitySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ServerFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is City city)
        {
            await viewModel.ChangeCityAsync(city.CityId);
        }
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSynchronizingPassword || DataContext is not ServerFormViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        viewModel.Entity.Clave = passwordBox.Password;
    }

    private void TogglePasswordClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ServerFormViewModel viewModel)
        {
            return;
        }

        _isSynchronizingPassword = true;
        PasswordBox.Password = viewModel.Entity.Clave ?? string.Empty;
        PasswordTextBox.Text = viewModel.Entity.Clave ?? string.Empty;
        _isSynchronizingPassword = false;
    }
}
