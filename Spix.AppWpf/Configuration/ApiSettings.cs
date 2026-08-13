namespace Spix.AppWpf.Configuration;

// Centraliza la direccion del Backend que consume la aplicacion desktop.
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; set; } = string.Empty;
}
