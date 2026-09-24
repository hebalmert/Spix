using System.Net;
using System.Text.Json;

namespace Spix.HttpService;

public class HttpResponseWrapper<T>
{
    public HttpResponseWrapper(
        T? response,
        bool error,
        HttpResponseMessage httpResponseMessage,
        string? errorMessage = null)
    {
        Response = response;
        Error = error;
        HttpResponseMessage = httpResponseMessage;
        ErrorMessage = errorMessage;
    }

    public bool Error { get; set; }
    public T? Response { get; set; }
    public HttpResponseMessage HttpResponseMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<string?> GetErrorMessageAsync()
    {
        if (!Error)
            return null;

        var statusCode = HttpResponseMessage.StatusCode;

        return statusCode switch
        {
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.BadRequest => await ReadBodyAsync(),
            HttpStatusCode.Unauthorized => "You are not authorized to access this resource.",
            HttpStatusCode.Forbidden => "Access to this resource is forbidden.",
            // El ExceptionHandlingMiddleware devuelve el mensaje ya traducido en el cuerpo del 500.
            HttpStatusCode.InternalServerError => await ReadBodyAsync("An internal server error occurred."),
            HttpStatusCode.RequestTimeout => "The request timed out.",
            HttpStatusCode.ServiceUnavailable => "The service is currently unavailable.",
            _ => "An unexpected error occurred."
        };
    }

    private async Task<string?> ReadBodyAsync(string? fallback = null)
    {
        var body = await HttpResponseMessage.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return fallback ?? body;
        }

        return LeerValidaciones(body) ?? body;
    }

    // Cuando el modelo no pasa las validaciones, ASP.NET no devuelve un mensaje sino un
    // JSON con todos los errores. Sin esto el usuario ve el JSON crudo en pantalla.
    private static string? LeerValidaciones(string body)
    {
        if (!body.TrimStart().StartsWith("{"))
        {
            return null;
        }

        try
        {
            using var documento = JsonDocument.Parse(body);
            var raiz = documento.RootElement;

            if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
            {
                var mensajes = errores.EnumerateObject()
                    .SelectMany(campo => campo.Value.EnumerateArray().Select(x => x.GetString()))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (mensajes.Count > 0)
                {
                    return string.Join(Environment.NewLine, mensajes);
                }
            }

            if (raiz.TryGetProperty("title", out var titulo) && titulo.ValueKind == JsonValueKind.String)
            {
                return titulo.GetString();
            }
        }
        catch (JsonException)
        {
            //No era un JSON de validacion: se devuelve el cuerpo tal cual
        }

        return null;
    }
}