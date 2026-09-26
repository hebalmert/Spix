using Spix.xLanguage.Resources;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Spix.AppWpf.SharedServices;

// El idioma en el que el Backend le responde al escritorio.
//
// El Backend elige el idioma por la cabecera Accept-Language y su valor por defecto es
// "en". El navegador manda esa cabecera solo, por eso la web sale en espanol; el
// HttpClient del escritorio no mandaba ninguna, asi que TODO lo que venia del servidor
// —los combos, los estados, los mensajes de error— llegaba en ingles.
//
// Aqui se fija esa cabecera y se puede cambiar en caliente desde la barra superior.
public class LanguageService
{
    public const string Spanish = "es";
    public const string English = "en";

    private readonly HttpClient _httpClient;

    // Arranca en ingles, que es lo que el Backend responde por defecto
    public string Current { get; private set; } = English;

    public bool IsSpanish => Current == Spanish;

    // La pantalla abierta tiene que volver a pedir sus datos: los textos ya cambiaron
    public event EventHandler? Changed;

    public LanguageService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        Aplicar(Current);
    }

    public void Set(string culture)
    {
        var idioma = culture == Spanish ? Spanish : English;

        if (idioma == Current)
        {
            return;
        }

        Current = idioma;

        Aplicar(idioma);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Toggle()
    {
        Set(IsSpanish ? English : Spanish);
    }

    // Un texto del MISMO archivo de traducciones que usan el Backend y la web
    // (Spix.xLanguage), en el idioma que este puesto.
    //
    // Hace falta para lo que el escritorio arma por su cuenta y no le llega traducido del
    // servidor: los nombres de los estados de un contrato, los eventos de su bitacora.
    // Escribirlos a mano seria reinventar una i18n que ya esta hecha.
    public string Text(string clave)
    {
        var texto = Resource.ResourceManager.GetString(clave, new CultureInfo(Current));

        //Si la clave no existe se devuelve tal cual: se ve en pantalla y se corrige
        return string.IsNullOrWhiteSpace(texto) ? clave : texto;
    }

    private void Aplicar(string idioma)
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(idioma));
    }
}
