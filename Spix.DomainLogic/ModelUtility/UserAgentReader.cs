namespace Spix.DomainLogic.ModelUtility;

//Lectura amigable del navegador que manda el equipo del firmante: "Chrome en Windows".
//Se usa en la hoja de certificado del PDF y en la pantalla, para que digan lo mismo.
public static class UserAgentReader
{
    public static string Describe(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return "-";

        var navegador = userAgent.Contains("Edg/") ? "Edge"
            : userAgent.Contains("OPR/") ? "Opera"
            : userAgent.Contains("Chrome/") ? "Chrome"
            : userAgent.Contains("Firefox/") ? "Firefox"
            : userAgent.Contains("Safari/") ? "Safari"
            : "Navegador desconocido";

        var sistema = userAgent.Contains("Windows") ? "Windows"
            : userAgent.Contains("Android") ? "Android"
            : userAgent.Contains("iPhone") ? "iPhone"
            : userAgent.Contains("iPad") ? "iPad"
            : userAgent.Contains("Mac OS") ? "Mac"
            : userAgent.Contains("Linux") ? "Linux"
            : "sistema desconocido";

        return $"{navegador} en {sistema}";
    }
}
