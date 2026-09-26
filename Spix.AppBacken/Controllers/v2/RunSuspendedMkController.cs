using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesContractDTO;

namespace Spix.AppBack.Controllers.v2;

//La version v2 del API es la del ESCRITORIO.
//
//El corte masivo le quita el acceso al cliente en el MikroTik, y el escritorio lo hace por
//la red LAN porque el cliente puede no tener IP publica. Por eso va partido en dos: el GET
//arma el lote de un equipo con sus datos de conexion, y el POST guarda cuando el equipo ya
//quedo escrito, recibiendo solo los contratos que el equipo SI acepto.
//
//Quien entra en el lote lo decide el servidor, no el escritorio: el POST vuelve a calcular
//la deuda y la cruza con la lista que le llega.
//
//El cierre del corte (run/finish) NO se duplica aqui: no toca el equipo, asi que el
//escritorio sigue usando el de v1.
//
//Vive en v2 a proposito: asi lo que ya funciona en v1 con Blazor no se toca ni se arriesga.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/runsuspendedmk")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class RunSuspendedMkController : ControllerBase
{
    private readonly IRunSuspendedMkServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public RunSuspendedMkController(IRunSuspendedMkServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    // El lote de un equipo: a quienes cortarles y que escribirles
    [HttpGet("{id}/server/{serverId}/setup")]
    public async Task<IActionResult> GetRunSetupAsync(Guid id, Guid serverId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetRunSetupAsync(id, serverId, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }

    // El equipo YA quedo escrito por la LAN: solo se guarda el rastro de lo que acepto
    [HttpPost("{id}/server/{serverId}")]
    public async Task<IActionResult> RunSaveAsync(Guid id, Guid serverId, CorteMkSaveDTO datos)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.RunSaveAsync(id, serverId, datos.Suspendidos, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_ServerError"].Value);
        }
    }
}
