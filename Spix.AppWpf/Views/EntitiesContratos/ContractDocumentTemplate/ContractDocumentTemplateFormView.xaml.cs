using Microsoft.Win32;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// El formulario que comparten Crear y Editar.
//
// Elegir el PDF es lo unico que no puede vivir en el ViewModel: el cuadro de archivos es
// del sistema. Se lee el archivo y se le entrega al formulario en el mismo Base64 que ya
// recibe el Backend, igual que hace SharedImagePicker con las fotos.
public partial class ContractDocumentTemplateFormView : UserControl
{
    public ContractDocumentTemplateFormView()
    {
        InitializeComponent();
    }

    private async void ElegirPdfClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ContractDocumentTemplateFormViewModel viewModel)
        {
            return;
        }

        var dialogo = new OpenFileDialog
        {
            Filter = "Documentos PDF|*.pdf",
            Multiselect = false
        };

        if (dialogo.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var bytes = File.ReadAllBytes(dialogo.FileName);

            viewModel.SetPdf(Convert.ToBase64String(bytes), Path.GetFileName(dialogo.FileName));
        }
        catch (Exception excepcion)
        {
            await viewModel.AvisarPdfAsync($"No fue posible leer el PDF. {excepcion.Message}");
        }
    }
}
