using Spix.HttpService;

namespace Spix.AppMaui.Services;

//Un solo sitio donde se mira si la respuesta vino mal, igual que el HttpResponseHandler
//de la web y del escritorio. Devuelve true cuando hubo error y ya se aviso.
public class ApiResponseHandler
{
    private readonly AlertService _alertService;

    public ApiResponseHandler(AlertService alertService)
    {
        _alertService = alertService;
    }

    public async Task<bool> HandleErrorAsync<T>(HttpResponseWrapper<T> response)
    {
        if (!response.Error)
        {
            return false;
        }

        var mensaje = await response.GetErrorMessageAsync();

        await _alertService.ErrorAsync(string.IsNullOrWhiteSpace(mensaje)
            ? "No fue posible completar la operacion."
            : mensaje.Trim().Trim('"'));

        return true;
    }
}
