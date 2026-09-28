using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesMk;
using Spix.DomainLogic.MkDTOs;

namespace Spix.AppBack.Controllers.v2;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/serverpppoelocal")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ServerPppoeLocalController : ControllerBase
{
    private readonly IMkConnectionServiceX _service;
    private readonly IStringLocalizer _localizer;

    public ServerPppoeLocalController(IMkConnectionServiceX service, IStringLocalizer localizer)
    {
        _service = service;
        _localizer = localizer;
    }

    [HttpGet("{serverId}")]
    public async Task<IActionResult> GetAsync(Guid serverId, [FromQuery] string? serviceName)
    {
        try
        {
            var user = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            return ResponseHelper.Format(await _service.GetPppoeLocalSetupAsync(serverId, serviceName, user.UserName));
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

    //Prepara el BORRADO: como llegar al equipo y los dos .id a quitar. El Service se
    //niega aqui mismo si el servidor tiene contratos PPPoE.
    [HttpGet("remove/{serverId}")]
    public async Task<IActionResult> GetRemoveAsync(Guid serverId)
    {
        try
        {
            var user = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            return ResponseHelper.Format(await _service.GetPppoeLocalRemoveAsync(serverId, user.UserName));
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

    //Limpia el espejo DESPUES de que el escritorio ya borro en el equipo
    [HttpDelete("{serverId}")]
    public async Task<IActionResult> ClearAsync(Guid serverId)
    {
        try
        {
            var user = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            return ResponseHelper.Format(await _service.ClearPppoeLocalAsync(serverId, user.UserName));
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

    [HttpPost]
    public async Task<IActionResult> SaveAsync(PppoeServerLocalSaveDTO datos)
    {
        try
        {
            var user = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            return ResponseHelper.Format(await _service.SavePppoeLocalAsync(datos, user.UserName));
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
