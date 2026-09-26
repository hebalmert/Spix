using Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Contractor;

// Recoge la foto que entrega el selector y aplica la regla visual de cuenta activa.
public partial class ContractorFormView : UserControl
{
    public ContractorFormView()
    {
        InitializeComponent();
    }

    // La foto llega en el mismo Base64 venga del disco o de la camara: al formulario le da igual
    private void PhotoSelected(object? sender, string base64)
    {
        if (DataContext is ContractorFormViewModel viewModel)
        {
            viewModel.SetPhoto(base64);
        }
    }

    private void ActiveChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ContractorFormViewModel viewModel && sender is CheckBox checkBox)
        {
            viewModel.SetActive(checkBox.IsChecked == true);
        }
    }
}
