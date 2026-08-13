using Spix.AppWpf.Services.Session;
using Spix.HttpService;
using System.Net;

namespace Spix.AppWpf.SharedServices;

// Interpreta respuestas HTTP con los mismos estados que el manejador central de Blazor.
public class HttpResponseHandler
{
    private readonly AlertService _alertService;
    private readonly IUserSessionService _userSessionService;

    public HttpResponseHandler(
        AlertService alertService,
        IUserSessionService userSessionService)
    {
        _alertService = alertService;
        _userSessionService = userSessionService;
    }

    public async Task<bool> HandleErrorAsync<T>(HttpResponseWrapper<T> responseHttp)
    {
        if (!responseHttp.Error)
        {
            return false;
        }

        var statusCode = responseHttp.HttpResponseMessage.StatusCode;
        var errorMessage = NormalizeMessage(
            await responseHttp.GetErrorMessageAsync());

        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
                _userSessionService.ClearSession();
                await _alertService.ErrorAsync(
                    "Acceso no autorizado",
                    "Tu sesion no es valida o ha vencido. Ingresa nuevamente.");
                return true;

            case HttpStatusCode.Forbidden:
                await _alertService.WarningAsync(
                    "Acceso restringido",
                    "No tienes permisos para realizar esta operacion.");
                return true;

            case HttpStatusCode.PaymentRequired:
                await _alertService.WarningAsync(
                    "Suscripcion requerida",
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "La prueba o suscripcion vencio. Selecciona un plan para continuar."
                        : errorMessage);
                return true;

            case HttpStatusCode.NotFound:
                await _alertService.WarningAsync(
                    "Registro no encontrado",
                    "La busqueda solicitada no fue encontrada.");
                return true;

            case HttpStatusCode.BadRequest:
                await _alertService.WarningAsync(
                    "Error de validacion",
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "La operacion solicitada no se puede completar en el estado actual."
                        : errorMessage);
                return true;

            case HttpStatusCode.UnprocessableEntity:
                await _alertService.WarningAsync(
                    "Datos no procesables",
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Los datos enviados no pueden ser procesados."
                        : errorMessage);
                return true;

            case HttpStatusCode.RequestTimeout:
            case HttpStatusCode.GatewayTimeout:
                await _alertService.WarningAsync(
                    "Tiempo de espera agotado",
                    "El servidor tardo demasiado en responder. Intenta nuevamente.");
                return true;

            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
                await _alertService.WarningAsync(
                    "Servicio no disponible",
                    "No fue posible comunicarse con el servicio. Intenta nuevamente en unos minutos.");
                return true;

            default:
                await _alertService.ErrorAsync(
                    "Error de respuesta",
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Ocurrio un error inesperado en el servidor."
                        : errorMessage);
                return true;
        }
    }

    // El Backend puede entregar mensajes JSON entre comillas y el alert debe mostrar texto limpio.
    private static string NormalizeMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message)
            ? string.Empty
            : message.Trim().Trim('"');
    }
}
