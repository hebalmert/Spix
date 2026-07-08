using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.Pagination;
using System.Security.Claims;

namespace Spix.AppBack.Controllers.EntitiesNet;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ipnets")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class IpNetsController : ControllerBase
{
    private readonly IIpNetServiceX _ipNetUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public IpNetsController(IIpNetServiceX ipNetUnitOfWork, IStringLocalizer localizer)
    {
        _ipNetUnitOfWork = ipNetUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet("loadCombo/{id?}")]
    public async Task<ActionResult<IEnumerable<Tax>>> GetComboAsync([FromRoute] Guid? id = null)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _ipNetUnitOfWork.ComboAsync(userClaimsInfo.UserName, id);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IpNet>>> GetAll([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        if (userClaimsInfo == null)
        {
            return BadRequest("Erro en el sistema de Usuarios");
        }

        var response = await _ipNetUnitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var response = await _ipNetUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<IpNet>> PutAsync(IpNet modelo)
    {
        var response = await _ipNetUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<IpNet>> PostAsync(IpNet modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

var response = await _ipNetUnitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost("pool")]
    public async Task<ActionResult<int>> PostPoolAsync(IpNetPoolCreateDTO modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

var response = await _ipNetUnitOfWork.AddPoolAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpPost("pool/delete")]
    public async Task<ActionResult<int>> DeletePoolAsync(IpNetPoolCreateDTO modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

var response = await _ipNetUnitOfWork.DeletePoolAsync(modelo, userClaimsInfo.UserName);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return BadRequest(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(Guid id)
    {
        var response = await _ipNetUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }
}
