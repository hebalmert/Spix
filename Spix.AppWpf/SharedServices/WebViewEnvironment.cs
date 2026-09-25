using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Reflection;
using System.Text;

namespace Spix.AppWpf.SharedServices;

// El navegador embebido de la aplicacion (los mapas y la camara), montado como lo monta la web.
//
// EL PROBLEMA QUE RESUELVE: OpenStreetMap reparte las teselas con servidores de
// voluntarios y bloquea a quien no se identifica; devuelve la tesela amarilla de
// "Access blocked" con un 403. En Blazor nunca pasa porque la pagina vive en
// https://spix.nexxtplanet.net y cada tesela sale con su cabecera Referer diciendo de
// donde viene. En el WPF el documento se creaba con NavigateToString, que deja la pagina
// en "about:blank": un navegador completo, con todas sus cabeceras, pero SIN origen y SIN
// Referer. Eso es justo lo que OSM corta.
//
// LA SOLUCION es la misma condicion de la web: el documento se escribe en una carpeta y
// se sirve desde un nombre de host propio del WebView2 (SetVirtualHostNameToFolderMapping).
// Con eso la pagina tiene un origen https de verdad y el propio navegador manda el Referer,
// sin que haya que tocar ninguna cabecera a mano.
//
// Ademas se le pone a la aplicacion un User-Agent que la identifica, que es lo otro que
// pide la politica de uso de teselas, y los datos del navegador (con su cache) viven en la
// carpeta del usuario y no dentro de bin\ - ahi se quedaban pegadas las teselas
// bloqueadas y el aviso seguia saliendo aunque OSM ya respondiera bien.
public static class WebViewEnvironment
{
    // El nombre con el que el WebView2 sirve nuestros documentos. No existe en la red:
    // lo resuelve el propio navegador contra la carpeta de abajo.
    private const string Host = "spix.mapa";

    // La direccion publica del producto. Sale de appsettings (ApiSettings:BaseUrl) y la
    // deja puesta App al arrancar; no se escribe a mano en ningun lado.
    public static string SitioWeb { get; set; } = string.Empty;

    private static CoreWebView2Environment? _entorno;

    // Deja el navegador listo. Se llama en lugar de EnsureCoreWebView2Async.
    public static async Task PrepararAsync(WebView2 navegador)
    {
        _entorno ??= await CoreWebView2Environment.CreateAsync(null, CarpetaDatos());

        //La carpeta tiene que existir antes de colgarla del host virtual
        Directory.CreateDirectory(CarpetaSitio());

        await navegador.EnsureCoreWebView2Async(_entorno);

        //El User-Agent se fija ANTES de navegar: solo afecta a lo que se pida despues
        navegador.CoreWebView2.Settings.UserAgent = UserAgent(navegador.CoreWebView2.Settings.UserAgent);

        //De aqui sale el origen real que hace que el navegador mande el Referer
        navegador.CoreWebView2.SetVirtualHostNameToFolderMapping(
            Host,
            CarpetaSitio(),
            CoreWebView2HostResourceAccessKind.Allow);
    }

    // Guarda el documento del mapa y devuelve la direccion con la que se abre.
    // Navigate siempre vuelve a navegar aunque la direccion sea la misma, asi que al
    // cambiar de punto se reescribe el archivo y se vuelve a pedir; no hace falta enganarlo.
    public static string PublicarDocumento(string archivo, string html)
    {
        var carpeta = CarpetaSitio();

        Directory.CreateDirectory(carpeta);
        File.WriteAllText(Path.Combine(carpeta, archivo), html, Encoding.UTF8);

        return $"https://{Host}/{archivo}";
    }

    // Donde se dejan los documentos que sirve el host virtual
    private static string CarpetaSitio()
    {
        return Path.Combine(RaizDeDatos(), "Mapa");
    }

    // Los datos del navegador van con los del usuario, no dentro de la carpeta de compilacion
    private static string CarpetaDatos()
    {
        return Path.Combine(RaizDeDatos(), "WebView2");
    }

    private static string RaizDeDatos()
    {
        var raiz = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(raiz, "Spix");
    }

    // Lo que pide la politica de OSM: quien es la aplicacion, que version y donde encontrarla.
    // Se AGREGA al que ya trae el navegador, no se reemplaza, para que los CDN que sirven
    // Leaflet sigan viendo un navegador normal.
    private static string UserAgent(string original)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0";
        var sitio = string.IsNullOrWhiteSpace(SitioWeb) ? string.Empty : $" (+{SitioWeb})";

        return $"{original} Spix-Desktop/{version}{sitio}";
    }
}
