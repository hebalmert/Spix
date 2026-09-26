using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.v2;

//La version v2 del API es la del ESCRITORIO.
//
//Suspender y reactivar tocan el MikroTik, y el escritorio lo hace por la red LAN porque el
//cliente puede no tener IP publica. Por eso el trabajo va partido en dos: los GET entregan
//con quien hablar y que escribir, y los POST guardan cuando el equipo ya quedo escrito.
//
//Las reglas —solo se suspende lo activo, el binding tiene que estar en bypassed, y al
//reactivar corre la validacion de integridad— se revisan en los dos lados: no las decide
//el escritorio.
//
//Vive en v2 a proposito: asi lo que ya funciona en v1 con Blazor no se toca ni se arriesga.
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/contractsuspendedmk")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractSuspendedMkController : ControllerBase
{
    private readonly IContractSuspendedMkServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ContractSuspendedMkController(IContractSuspendedMkServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    // Con que bindings hablar y que ponerles para suspender
    [HttpGet("{id}/suspend")]
    public async Task<IActionResult> GetSuspendSetupAsync(Guid id)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetSuspendSetupAsync(id, userClaimsInfo.UserName);

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

    // Lo mismo para devolver el acceso
    [HttpGet("{id}/activate")]
    public async Task<IActionResult> GetReactivateSetupAsync(Guid id)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.GetReactivateSetupAsync(id, userClaimsInfo.UserName);

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

    // De aqui para abajo el equipo YA quedo escrito por la LAN: solo se guarda el rastro

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendSaveAsync(Guid id, [FromQuery] string? motivo)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.SuspendSaveAsync(id, motivo, userClaimsInfo.UserName);

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

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ReactivateSaveAsync(Guid id)
    {
        try
        {
            var userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.ReactivateSaveAsync(id, userClaimsInfo.UserName);

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
