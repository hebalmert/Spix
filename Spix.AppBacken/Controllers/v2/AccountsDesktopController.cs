using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSecure;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.v2;

//El login del SOFTWARE DE PC.
//
//Por que existe aparte: el escritorio se paga como un derecho del plan. Este login hace lo
//mismo que el de v1 y ademas comprueba que el plan de la corporacion incluya el software de
//PC; si no lo incluye, no entrega token y dice por que. Ademas marca el token con el claim
//Client=Desktop, que el filtro de suscripcion lee en CADA peticion para cortarle el paso si
//mas adelante le quitan ese derecho al plan, sin esperar a que el token venza.
//
//El login de v1 no se toca: la web sigue entrando por ahi exactamente igual.
//
//NO se entrega cookie de refresco como en v1: el escritorio no la usa, se queda con el token
//hasta que vence y vuelve a pedir credenciales.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/accounts")]
[ApiController]
public class AccountsDesktopController : ControllerBase
{
    private readonly IAccountServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public AccountsDesktopController(IAccountServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO modelo)
    {
        try
        {
            var response = await _unitOfWork.LoginDesktopAsync(modelo);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }
}
