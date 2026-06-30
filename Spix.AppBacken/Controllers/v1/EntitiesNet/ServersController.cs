using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.EntitiesNet;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ServersController : ControllerBase
{
    private readonly IServerServiceX _serverUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public ServersController(IServerServiceX serverUnitOfWork, IStringLocalizer localizer)
    {
        _serverUnitOfWork = serverUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet("loadCombo/{id?}")]
    public async Task<ActionResult<IEnumerable<Server>>> GetComboAsync([FromRoute] Guid? id = null)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _serverUnitOfWork.ComboAsync(userClaimsInfo.UserName, id);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Server>>> GetAll([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _serverUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var response = await _serverUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<Server>> PutAsync(Server modelo)
    {
        var response = await _serverUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<Server>> PostAsync(Server modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _serverUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(Guid id)
    {
        var response = await _serverUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }
}
