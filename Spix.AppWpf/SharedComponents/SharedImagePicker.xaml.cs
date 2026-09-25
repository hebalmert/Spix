using FontAwesome.Net.Generators;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Spix.AppWpf.SharedServices;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Spix.AppWpf.SharedComponents;

// Tomar la foto con la camara o subirla del disco, igual que el InputImageGeneral de la web.
//
// La camara la pone el navegador que ya trae la aplicacion, con el MISMO getUserMedia que
// usa la web: no hace falta ninguna libreria de captura ni cambiar el proyecto. El
// documento se sirve desde el host propio (WebViewEnvironment) porque el navegador solo
// entrega la camara a una pagina con origen seguro.
//
// Entrega la foto en el mismo Base64 que ya recibe el Backend, asi que el formulario la
// guarda igual venga del disco o de la camara.
public partial class SharedImagePicker : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SharedImagePicker),
        new PropertyMetadata("Foto"));

    // El icono que se ve mientras no hay foto: persona para un cliente, camion para un proveedor
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(FontAwesomeIcon), typeof(SharedImagePicker),
        new PropertyMetadata(FontAwesomeIcon.User));

    // La foto que ya tiene el registro, cuando se esta editando
    public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register(
        nameof(ImageUrl), typeof(string), typeof(SharedImagePicker));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public FontAwesomeIcon Icon
    {
        get => (FontAwesomeIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string? ImageUrl
    {
        get => (string?)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    // La foto elegida, en el Base64 que espera el Backend
    public event EventHandler<string>? ImageSelected;

    private bool _navegadorListo;
    private bool _filmando;

    public SharedImagePicker()
    {
        InitializeComponent();

        //Si el modal se cierra con la camara encendida, el equipo la seguiria usando
        Unloaded += (_, _) => _ = ApagarAsync();
    }

    // El mismo boton enciende la camara y, ya encendida, dispara
    private async void CamaraClick(object sender, RoutedEventArgs e)
    {
        if (_filmando)
        {
            await CapturarAsync();
            return;
        }

        await EncenderAsync();
    }

    private void DiscoClick(object sender, RoutedEventArgs e)
    {
        var dialogo = new OpenFileDialog
        {
            Filter = "Imagenes|*.jpg;*.jpeg;*.png;*.webp",
            Multiselect = false
        };

        if (dialogo.ShowDialog() != true)
        {
            return;
        }

        //Si habia camara encendida se apaga: la foto ya salio de otro lado
        _ = ApagarAsync();

        try
        {
            var bytes = File.ReadAllBytes(dialogo.FileName);

            Entregar(Convert.ToBase64String(bytes));
        }
        catch (Exception excepcion)
        {
            Avisar($"No fue posible leer la imagen. {excepcion.Message}");
        }
    }

    private async Task EncenderAsync()
    {
        try
        {
            await PrepararNavegadorAsync();

            _filmando = true;
            MarcoCamara.Visibility = Visibility.Visible;
            BotonCamara.Content = "Capturar";
            Avisar(null);

            await Camara.CoreWebView2.ExecuteScriptAsync("window.spixCamara.encender();");
        }
        catch (Exception excepcion)
        {
            await ApagarAsync();
            Avisar($"No fue posible abrir la camara. {excepcion.Message}");
        }
    }

    private async Task CapturarAsync()
    {
        try
        {
            var respuesta = await Camara.CoreWebView2.ExecuteScriptAsync("window.spixCamara.tomar();");

            //ExecuteScriptAsync devuelve el valor en JSON, hay que sacarlo de ahi
            var datos = JsonSerializer.Deserialize<string>(respuesta);

            await ApagarAsync();

            if (string.IsNullOrWhiteSpace(datos))
            {
                Avisar("No se pudo tomar la foto. Revise que el equipo tenga camara y que este permitida.");
                return;
            }

            //Llega como "data:image/jpeg;base64,xxxx": al Backend solo va lo de despues de la coma
            var base64 = datos[(datos.IndexOf(',') + 1)..];

            Entregar(base64);
        }
        catch (Exception excepcion)
        {
            await ApagarAsync();
            Avisar($"No fue posible tomar la foto. {excepcion.Message}");
        }
    }

    private async Task ApagarAsync()
    {
        _filmando = false;
        MarcoCamara.Visibility = Visibility.Collapsed;
        BotonCamara.Content = "Camara";

        if (!_navegadorListo)
        {
            return;
        }

        try
        {
            await Camara.CoreWebView2.ExecuteScriptAsync("window.spixCamara.apagar();");
        }
        catch (Exception)
        {
            //Apagar una camara que ya no esta no es un problema que valga contar
        }
    }

    // Deja la foto en la vista previa y se la pasa al formulario
    private void Entregar(string base64)
    {
        Vista.Source = DesdeBase64(base64);
        Avisar(null);

        ImageSelected?.Invoke(this, base64);
    }

    private void Avisar(string? mensaje)
    {
        Aviso.Text = mensaje;
        Aviso.Visibility = string.IsNullOrWhiteSpace(mensaje) ? Visibility.Collapsed : Visibility.Visible;
    }

    private static BitmapImage? DesdeBase64(string base64)
    {
        try
        {
            var imagen = new BitmapImage();
            imagen.BeginInit();
            imagen.CacheOption = BitmapCacheOption.OnLoad;
            imagen.StreamSource = new MemoryStream(Convert.FromBase64String(base64));
            imagen.EndInit();
            imagen.Freeze();

            return imagen;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task PrepararNavegadorAsync()
    {
        if (_navegadorListo)
        {
            return;
        }

        await WebViewEnvironment.PrepararAsync(Camara);

        //El navegador pregunta por la camara como se la pregunta a una pagina web. Aqui la
        //pagina es nuestra, asi que se concede sin molestar al usuario con otro permiso.
        Camara.CoreWebView2.PermissionRequested += (_, argumentos) =>
        {
            if (argumentos.PermissionKind == CoreWebView2PermissionKind.Camera)
            {
                argumentos.State = CoreWebView2PermissionState.Allow;
            }
        };

        var listo = new TaskCompletionSource();

        void Terminada(object? emisor, CoreWebView2NavigationCompletedEventArgs argumentos)
        {
            Camara.CoreWebView2.NavigationCompleted -= Terminada;
            listo.TrySetResult();
        }

        Camara.CoreWebView2.NavigationCompleted += Terminada;
        Camara.CoreWebView2.Navigate(WebViewEnvironment.PublicarDocumento("camara.html", Documento()));

        await listo.Task;

        _navegadorListo = true;
    }

    // El documento que vive dentro del navegador: el mismo getUserMedia de la web
    private static string Documento()
    {
        return """
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="utf-8" />
                <style>
                    html, body { width: 100%; height: 100%; margin: 0; overflow: hidden; background: #E8EFFA; }
                    video { width: 100%; height: 100%; object-fit: cover; }
                </style>
            </head>
            <body>
                <video id="camara" autoplay playsinline muted></video>
                <script>
                window.spixCamara = {
                    flujo: null,

                    encender: async function () {
                        window.spixCamara.apagar();
                        window.spixCamara.flujo = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
                        document.getElementById("camara").srcObject = window.spixCamara.flujo;
                    },

                    // Se sueltan TODAS las pistas: si no, la luz de la camara se queda encendida
                    apagar: function () {
                        if (!window.spixCamara.flujo) { return; }

                        window.spixCamara.flujo.getTracks().forEach(pista => pista.stop());
                        window.spixCamara.flujo = null;
                        document.getElementById("camara").srcObject = null;
                    },

                    // Recorta el cuadrado del centro, que es lo que se ve en el circulo
                    tomar: function () {
                        const video = document.getElementById("camara");
                        const lado = Math.min(video.videoWidth, video.videoHeight);

                        if (!lado) { return ""; }

                        const lienzo = document.createElement("canvas");
                        lienzo.width = lado;
                        lienzo.height = lado;

                        lienzo.getContext("2d").drawImage(video,
                            (video.videoWidth - lado) / 2, (video.videoHeight - lado) / 2, lado, lado,
                            0, 0, lado, lado);

                        return lienzo.toDataURL("image/jpeg", 0.9);
                    }
                };
                </script>
            </body>
            </html>
            """;
    }
}
