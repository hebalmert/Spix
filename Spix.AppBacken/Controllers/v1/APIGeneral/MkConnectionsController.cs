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
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
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
}