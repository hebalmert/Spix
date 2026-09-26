using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;
using Spix.DomainLogic.EnumTypes;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractClient;

// El documento y su firma.
//
// La firma no puede vivir en el ViewModel: son trazos de un InkCanvas, que es un control.
// Aqui se convierten a la imagen en Base64 que espera el Backend y se le entregan.
public partial class ContractDocumentDialogView : UserControl, ISharedModalContent
{
    //Lo que mide la imagen de la firma que se manda, en puntos
    private const int AnchoFirma = 460;

    private const int AltoFirma = 140;

    private readonly ContractDocumentDialogViewModel _viewModel;
    private Guid _contractClientId;
    private ContractDocumentType _documentType;
    private bool _loaded;

    public ContractDocumentDialogView(ContractDocumentDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        //El comando de guardar vive aqui porque necesita los trazos del lienzo
        _viewModel.DocumentReady += MostrarDocumento;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is null)
        {
            return;
        }

        if (parameters.TryGetValue("ContractClientId", out var contrato) && contrato is Guid id)
        {
            _contractClientId = id;
        }

        if (parameters.TryGetValue("DocumentType", out var tipo) && tipo is ContractDocumentType documento)
        {
            _documentType = documento;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        await _viewModel.InitializeAsync(_contractClientId, _documentType);
    }

    // El PDF lo muestra el navegador que la aplicacion ya trae
    private async void MostrarDocumento(object? sender, string url)
    {
        try
        {
            await WebViewEnvironment.PrepararAsync(DocumentBrowser);

            DocumentBrowser.CoreWebView2.Navigate(url);
        }
        catch (Exception)
        {
            //Si el visor no arranca, el documento igual quedo generado en el servidor
        }
    }

    private void ClearSignatureClick(object sender, RoutedEventArgs e)
    {
        SignaturePad.Strokes.Clear();
    }

    // Lo llama el boton de guardar del pie del modal
    [RelayCommand]
    private async Task SignAsync()
    {
        await _viewModel.SignAsync(FirmaEnBase64());
    }

    // Los trazos del lienzo, convertidos a la imagen que espera el Backend
    private string? FirmaEnBase64()
    {
        if (SignaturePad.Strokes.Count == 0)
        {
            return null;
        }

        var destino = new RenderTargetBitmap(AnchoFirma, AltoFirma, 96, 96, PixelFormats.Pbgra32);

        //Fondo blanco: sin el, la firma sale sobre transparente y se pierde en el PDF
        var dibujo = new DrawingVisual();

        using (var contexto = dibujo.RenderOpen())
        {
            contexto.DrawRectangle(Brushes.White, null, new Rect(0, 0, AnchoFirma, AltoFirma));

            SignaturePad.Strokes.Draw(contexto);
        }

        destino.Render(dibujo);

        var codificador = new PngBitmapEncoder();
        codificador.Frames.Add(BitmapFrame.Create(destino));

        using var memoria = new MemoryStream();
        codificador.Save(memoria);

        return Convert.ToBase64String(memoria.ToArray());
    }
}
