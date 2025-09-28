using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.Pagination;
using Spix.UnitOfWork.InterfacesEntitiesData;

namespace Spix.AppBack.Controllers.EntitiesData;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/securities")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class SecuritiesController : ControllerBase
{
    private readonly ISecurityUnitOfWork _securityUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public SecuritiesController(ISecurityUnitOfWork securityUnitOfWork, IStringLocalizer localizer)
    {
        _securityUnitOfWork = securityUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Security>>> GetAll([FromQuery] PaginationDTO pagination)
    {
        var response = await _securityUnitOfWork.GetAsync(pagination);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var response = await _securityUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<Security>> PutAsync(Security modelo)
    {
        var response = await _securityUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<Security>> PostAsync(Security modelo)
    {
        var response = await _securityUnitOfWork.AddAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(int id)
    {
        var response = await _securityUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }
}