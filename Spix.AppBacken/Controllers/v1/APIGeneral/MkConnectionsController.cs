using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfacesMk;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBacken.Controllers.v1.APIGeneral;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mkconnections")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class MkConnectionsController : ControllerBase
{
    private readonly IStringLocalizer _localizer;
    private readonly IMkConnectionServiceX _mkConnection;

    public MkConnectionsController(IStringLocalizer localizer, IMkConnectionServiceX mkConnection)
    {
        _localizer = localizer;
        _mkConnection = mkConnection;
    }

    [HttpGet("mkchecks/{id}")]
    public async Task<IActionResult> GetInterfaces(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }
        var response = await _mkConnection.CheckConnectionAsync(id, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    //Las interfaces del equipo, ya listas para pintar y con el neutro en la posicion 0
    [HttpGet("interfaces/{id}")]
    public async Task<IActionResult> Interfaces(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }
        var response = await _mkConnection.InterfacesComboAsync(id, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    //Crea el servidor PPPoE y su perfil en el equipo. Una sola vez por servidor.
    //Deshace la configuracion PPPoE del equipo. El Service se niega si hay contratos.
    [HttpDelete("pppoeserver/{id}")]
    public async Task<IActionResult> DeletePppoeServer(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }
        var response = await _mkConnection.DeletePppoeServerAsync(id, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpPost("pppoeserver/{id}")]
    public async Task<IActionResult> CreatePppoeServer(Guid id, [FromQuery] string? serviceName)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }
        var response = await _mkConnection.CreatePppoeServerAsync(id, serviceName, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }
}