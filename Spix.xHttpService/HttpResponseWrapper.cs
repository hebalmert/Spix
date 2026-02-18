using System.Net;

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
            HttpStatusCode.BadRequest => await HttpResponseMessage.Content.ReadAsStringAsync(),
            HttpStatusCode.Unauthorized => "You are not authorized to access this resource.",
            HttpStatusCode.Forbidden => "Access to this resource is forbidden.",
            HttpStatusCode.InternalServerError => "An internal server error occurred.",
            HttpStatusCode.RequestTimeout => "The request timed out.",
            HttpStatusCode.ServiceUnavailable => "The service is currently unavailable.",
            _ => "An unexpected error occurred."
        };
    }
}