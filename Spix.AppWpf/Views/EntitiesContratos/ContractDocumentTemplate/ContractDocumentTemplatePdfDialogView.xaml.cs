using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// El PDF lo arma el SERVIDOR y aqui solo se muestra, en el navegador que la aplicacion ya trae.
public partial class ContractDocumentTemplatePdfDialogView : UserControl, ISharedModalContent
{
    private readonly ContractDocumentTemplatePdfViewModel _viewModel;
    private string? _pdfUrl;
    private bool _isLoaded;

    public ContractDocumentTemplatePdfDialogView(ContractDocumentTemplatePdfViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += CargarDialogo;
        Unloaded += CerrarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("PdfUrl", out var valor) == true && valor is string url)
        {
            _pdfUrl = url;
        }
    }

    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || string.IsNullOrWhiteSpace(_pdfUrl))
        {
            return;
        }

        _isLoaded = true;
        _viewModel.IsLoading = true;

        try
        {
            await WebViewEnvironment.PrepararAsync(DocumentBrowser);

            DocumentBrowser.CoreWebView2.Navigate(_pdfUrl);
        }
        catch (Exception)
        {
            //Si el visor no arranca, el documento igual existe en el servidor
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    // Al cerrar el visor se suelta el navegador; si no, queda vivo su proceso con el PDF dentro
    private void CerrarDialogo(object sender, RoutedEventArgs e)
    {
        try
        {
            DocumentBrowser.Dispose();
        }
        catch (Exception)
        {
            //Cerrar un visor que ya no esta no es un problema que valga contar
        }
    }
}
