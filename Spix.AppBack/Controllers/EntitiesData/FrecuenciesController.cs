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
[Route("api/v{version:apiVersion}/frecuencies")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class FrecuenciesController : ControllerBase
{
    private readonly IFrecuencyUnitOfWork _frecuencyUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public FrecuenciesController(IFrecuencyUnitOfWork frecuencyUnitOfWork, IStringLocalizer localizer)
    {
        _frecuencyUnitOfWork = frecuencyUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Frecuency>>> GetAll([FromQuery] PaginationDTO pagination)
    {
        var response = await _frecuencyUnitOfWork.GetAsync(pagination);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var response = await _frecuencyUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<Frecuency>> PutAsync(Frecuency modelo)
    {
        var response = await _frecuencyUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<Frecuency>> PostAsync(Frecuency modelo)
    {
        var response = await _frecuencyUnitOfWork.AddAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(int id)
    {
        var response = await _frecuencyUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }
}