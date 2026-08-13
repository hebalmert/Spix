using Spix.AppWpf.ViewModels.EntitiesNet.Node;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesNet.Node;

// Mantiene los selects dependientes y el campo de clave sincronizados con el formulario.
public partial class NodeFormView : UserControl
{
    private bool _isSynchronizingPassword;

    public NodeFormView()
    {
        InitializeComponent();
    }

    private async void StateSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is NodeFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is State state)
        {
            await viewModel.ChangeStateAsync(state.StateId);
        }
    }

    private async void CitySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is NodeFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is City city)
        {
            await viewModel.ChangeCityAsync(city.CityId);
        }
    }

    private async void MarkSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is NodeFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is Mark mark)
        {
            await viewModel.ChangeMarkAsync(mark.MarkId);
        }
    }

    private async void FrecuencyTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is NodeFormViewModel viewModel && !viewModel.IsInitializing && e.AddedItems.Count > 0 && e.AddedItems[0] is IntItemModel item)
        {
            await viewModel.ChangeFrecuencyTypeAsync(item.Value);
        }
    }

    private async void CoordinatesLostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is NodeFormViewModel viewModel && sender is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            await viewModel.UpdateCoordinatesAsync(textBox.Text);
        }
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSynchronizingPassword || DataContext is not NodeFormViewModel viewModel || sender is not PasswordBox passwordBox)
        {
            return;
        }

        viewModel.Entity.Clave = passwordBox.Password;
    }

    private void TogglePasswordClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not NodeFormViewModel viewModel)
        {
            return;
        }

        _isSynchronizingPassword = true;
        PasswordBox.Password = viewModel.Entity.Clave ?? string.Empty;
        PasswordTextBox.Text = viewModel.Entity.Clave ?? string.Empty;
        _isSynchronizingPassword = false;
    }
}
