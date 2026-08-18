using Microsoft.Win32;
using Spix.AppWpf.ViewModels.EntitiesOper.Client;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesOper.Client;

// Permite seleccionar una foto local y aplica la regla visual de cuenta activa.
public partial class ClientFormView : UserControl
{
    public ClientFormView()
    {
        InitializeComponent();
    }

    private void SelectPhotoClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ClientFormViewModel viewModel)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Imagenes|*.jpg;*.jpeg;*.png;*.webp",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            viewModel.SelectPhoto(dialog.FileName);
        }
    }

    private void ActiveChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientFormViewModel viewModel && sender is CheckBox checkBox)
        {
            viewModel.SetActive(checkBox.IsChecked == true);
        }
    }
}
