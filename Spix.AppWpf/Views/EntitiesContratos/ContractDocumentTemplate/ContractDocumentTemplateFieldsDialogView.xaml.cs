using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;

// El editor visual de campos dentro del navegador embebido.
//
// El dibujado es el MISMO de la web (pdf.js + pdfFieldEditor.js). Lo unico que cambia es el
// transporte:
//   C# -> JS : CoreWebView2.ExecuteScriptAsync
//   JS -> C# : window.chrome.webview.postMessage y el evento WebMessageReceived
//
// El .js y la pagina que lo carga se publican en tiempo de ejecucion con
// WebViewEnvironment.PublicarDocumento, que los sirve desde el host propio del WebView2:
// asi la pagina tiene un origen https de verdad y pdf.js se puede bajar del CDN.
public partial class ContractDocumentTemplateFieldsDialogView : UserControl, ISharedModalContent
{
    private const string HostId = "spixPdfFieldEditorHost";
    private const string ArchivoScript = "pdfFieldEditor.js";
    private const string ArchivoPagina = "pdfeditor.html";

    //El mismo camelCase que usa Blazor al mandarle los campos al editor
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ContractDocumentTemplateFieldsViewModel _viewModel;
    private Guid _id;
    private bool _isLoaded;
    private bool _navegadorListo;

    public ContractDocumentTemplateFieldsDialogView(ContractDocumentTemplateFieldsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.DocumentReady += DibujarDocumento;
        _viewModel.FieldsChanged += EnviarCampos;
        _viewModel.ScrollToFieldRequested += LlevarAlCampo;

        Loaded += CargarDialogo;
        Unloaded += CerrarDialogo;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("Id", out var valor) == true && valor is Guid id)
        {
            _id = id;
        }
    }

    // Primero se deja listo el navegador y despues se pide la plantilla: si fuera al reves,
    // el PDF llegaria antes de que haya donde dibujarlo.
    private async void CargarDialogo(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _id == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;

        try
        {
            await PrepararNavegadorAsync();
        }
        catch (Exception excepcion)
        {
            //Sin navegador no hay editor, pero la pantalla tiene que quedar utilizable:
            //si el velo de carga se queda puesto no se ve ni el boton de salir.
            _viewModel.IsLoading = false;

            await _viewModel.AvisarAsync($"No fue posible abrir el editor. {excepcion.Message}");
            return;
        }

        await _viewModel.InitializeAsync(_id);
    }

    // Al cerrar el modal se suelta el navegador: con el se va el PDF que pdf.js tiene en
    // memoria y el proceso que el WebView2 levanta. Si no se suelta, cada vez que se abre
    // el editor queda un navegador vivo de mas.
    private void CerrarDialogo(object sender, RoutedEventArgs e)
    {
        _viewModel.DocumentReady -= DibujarDocumento;
        _viewModel.FieldsChanged -= EnviarCampos;
        _viewModel.ScrollToFieldRequested -= LlevarAlCampo;

        if (!_navegadorListo)
        {
            return;
        }

        _navegadorListo = false;

        try
        {
            EditorBrowser.CoreWebView2.WebMessageReceived -= MensajeRecibido;
            EditorBrowser.Dispose();
        }
        catch (Exception)
        {
            //Cerrar un editor que ya no esta no es un problema que valga contar
        }
    }

    // El PDF llega en Base64 y se le entrega al editor
    private async void DibujarDocumento(object? sender, PdfEditorDocument documento)
    {
        if (!_navegadorListo)
        {
            return;
        }

        try
        {
            var base64 = JsonSerializer.Serialize(documento.Base64, Json);
            var soloLectura = documento.ReadOnly ? "true" : "false";

            await EditorBrowser.CoreWebView2.ExecuteScriptAsync($"window.spixEditor.cargar({base64}, {soloLectura});");
        }
        catch (Exception excepcion)
        {
            await _viewModel.AvisarAsync($"No fue posible mostrar el PDF: {excepcion.Message}");
        }
    }

    private async void EnviarCampos(object? sender, EventArgs e)
    {
        await EnviarCamposAsync();
    }

    // El usuario eligio un campo en la lista: el PDF se mueve hasta el, igual que en la web
    private async void LlevarAlCampo(object? sender, string key)
    {
        if (!_navegadorListo)
        {
            return;
        }

        try
        {
            var clave = JsonSerializer.Serialize(key, Json);

            await EditorBrowser.CoreWebView2.ExecuteScriptAsync($"window.spixPdfFieldEditor.scrollToField({clave});");
        }
        catch (Exception)
        {
            //No poder mover el PDF hasta el campo no estropea nada de lo que se esta editando
        }
    }

    private async Task EnviarCamposAsync()
    {
        if (!_navegadorListo)
        {
            return;
        }

        try
        {
            var foto = _viewModel.BuildEditorFields();

            var campos = JsonSerializer.Serialize(foto.Fields, Json);
            var elegido = JsonSerializer.Serialize(foto.SelectedKey, Json);

            await EditorBrowser.CoreWebView2.ExecuteScriptAsync(
                $"window.spixPdfFieldEditor.setFields({campos}, {elegido}, {foto.PlacingType});");
        }
        catch (Exception)
        {
            //Si el editor todavia no esta, el proximo cambio lo vuelve a intentar
        }
    }

    // Lo que el usuario hace sobre el PDF llega por aqui
    private async void MensajeRecibido(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var mensaje = JsonDocument.Parse(e.WebMessageAsJson);
            var raiz = mensaje.RootElement;

            if (!raiz.TryGetProperty("accion", out var accion))
            {
                return;
            }

            switch (accion.GetString())
            {
                case "Listo":
                    await EnviarCamposAsync();
                    break;

                case "PlaceField":
                    _viewModel.PlaceField(
                        Entero(raiz, "pageNumber"),
                        Numero(raiz, "positionX"),
                        Numero(raiz, "positionY"));
                    break;

                case "MoveField":
                    _viewModel.MoveField(
                        Texto(raiz, "key"),
                        Numero(raiz, "positionX"),
                        Numero(raiz, "positionY"),
                        NumeroOpcional(raiz, "width"),
                        NumeroOpcional(raiz, "height"));
                    break;

                case "SelectField":
                    _viewModel.SelectField(Texto(raiz, "key"));
                    break;

                case "Error":
                    await _viewModel.AvisarAsync($"No fue posible mostrar el PDF: {Texto(raiz, "mensaje")}");
                    break;
            }
        }
        catch (Exception)
        {
            //Un mensaje que no se entiende no puede tumbar la pantalla
        }
    }

    private async Task PrepararNavegadorAsync()
    {
        if (_navegadorListo)
        {
            return;
        }

        await WebViewEnvironment.PrepararAsync(EditorBrowser);

        EditorBrowser.CoreWebView2.WebMessageReceived += MensajeRecibido;

        //El .js y la pagina se escriben en la carpeta que sirve el host virtual
        WebViewEnvironment.PublicarDocumento(ArchivoScript, PdfFieldEditorScript.Codigo);
        var direccion = WebViewEnvironment.PublicarDocumento(ArchivoPagina, Documento());

        var listo = new TaskCompletionSource();

        void Terminada(object? emisor, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs argumentos)
        {
            EditorBrowser.CoreWebView2.NavigationCompleted -= Terminada;
            listo.TrySetResult();
        }

        EditorBrowser.CoreWebView2.NavigationCompleted += Terminada;
        EditorBrowser.CoreWebView2.Navigate(direccion);

        await listo.Task;

        _navegadorListo = true;
    }

    // La pagina que vive dentro del navegador: el host del PDF y el editor de la web.
    // El envoltorio spixEditor espera a que el PDF termine de dibujarse y recien ahi avisa,
    // porque los campos no se pueden mandar antes de que existan las paginas.
    private static string Documento()
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="utf-8" />
                <style>
                    html, body { width: 100%; height: 100%; margin: 0; background: #E8EFFA; }
                    body { overflow-y: auto; padding: 14px; box-sizing: border-box; }
                    #{{HostId}} { width: 100%; }
                </style>
                <script src="{{ArchivoScript}}"></script>
            </head>
            <body>
                <div id="{{HostId}}" class="spix-pdf-host"></div>
                <script>
                window.spixEditor = {
                    cargar: async function (base64, soloLectura) {
                        try {
                            await window.spixPdfFieldEditor.load("{{HostId}}", base64, soloLectura);
                            window.chrome.webview.postMessage({ accion: "Listo" });
                        } catch (error) {
                            window.chrome.webview.postMessage({ accion: "Error", mensaje: error.message });
                        }
                    }
                };
                </script>
            </body>
            </html>
            """;
    }

    private static string Texto(JsonElement raiz, string nombre)
    {
        return raiz.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int Entero(JsonElement raiz, string nombre)
    {
        return raiz.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.Number
            ? (int)valor.GetDouble()
            : 0;
    }

    private static double Numero(JsonElement raiz, string nombre)
    {
        return raiz.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.Number
            ? valor.GetDouble()
            : 0;
    }

    private static double? NumeroOpcional(JsonElement raiz, string nombre)
    {
        return raiz.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.Number
            ? valor.GetDouble()
            : null;
    }
}
