using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.DomainLogic.AppResponses;
using Spix.xNetwork.PingHelper;

namespace Spix.AppBacken.Controllers.v1.APIGeneral;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checksnet")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class CheckNetsController : ControllerBase
{
    private readonly IPingControl _pingControl;
    private readonly IStringLocalizer _localizer;

    public CheckNetsController(IPingControl pingControl, IStringLocalizer localizer)
    {
        _pingControl = pingControl;
        _localizer = localizer;
    }

    [HttpGet("ping/{host}")]
    public async Task<ActionResult<PingResult>> PingHost(string host)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _pingControl.PingAsync(host);

        //Que el host no conteste NO es un error del API: es el resultado del ping y el front
        //debe poder mostrarlo (tiempos, perdidos, timeouts). Solo se responde con error cuando
        //el ping ni siquiera se pudo ejecutar (Result viene en null), y ahi va solo el mensaje,
        //nunca el objeto entero serializado.
        if (response.Result == null)
            return BadRequest(response.Message);

        return Ok(response);
    }
}