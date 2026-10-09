namespace Spix.AppMaui.Configuration;

//La direccion del Backend vive en appsettings.json, igual que en el escritorio:
//no se hardcodea en el codigo.
public class ApiSettings
{
    public const string FileName = "appsettings.json";

    public string BaseUrl { get; set; } = string.Empty;
}
