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
//La reactivacion masiva le devuelve el acceso al cliente en el MikroTik, y el escritorio lo
//hace por la red LAN porque el cliente puede no tener IP publica. Por eso va partida en dos:
//el GET entrega el lote de un equipo con sus datos de conexion, y el POST guarda cuando el
//equipo ya quedo escrito, recibiendo solo los contratos que el equipo SI acepto.
//
//Quien entra en el lote lo decide el servidor, no el escritorio: el POST vuelve a calcularlo
//y lo cruza con la lista que le llega.
//
//Vive en v2 a proposito: asi lo que ya funciona en v1 con Blazor no se toca ni se arriesga.
//Y va solo para Administrator, igual que v1/activation: aqui no se afloja nada.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/activationmk")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ActivationMkController : ControllerBase
{
    private readonly IActivationMkServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ActivationMkController(IActivationMkServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    // El lote de un equipo: con quien hablar, que ponerles y a quienes
    [HttpGet("server/{serverId}/activate")]
    public async Task<IActionResult> GetActivateSetupAsync(Guid serverId)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetActivateSetupAsync(serverId, userClaimsInfo.UserName);

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
    [HttpPost("server/{serverId}/activate")]
    public async Task<IActionResult> ActivateSaveAsync(Guid serverId, ActivationMkSaveDTO datos)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.ActivateSaveAsync(serverId, datos.Activados, userClaimsInfo.UserName);

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
