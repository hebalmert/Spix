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
[Route("api/v{version:apiVersion}/frecuencytypes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class FrecuencyTypesController : ControllerBase
{
    private readonly IFrecuencyTypeUnitOfWork _frecuencyTypeUnitOfWork;
    private readonly IStringLocalizer _localizer;

    public FrecuencyTypesController(IFrecuencyTypeUnitOfWork frecuencyTypeUnitOfWork, IStringLocalizer localizer)
    {
        _frecuencyTypeUnitOfWork = frecuencyTypeUnitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FrecuencyType>>> GetAll([FromQuery] PaginationDTO pagination)
    {
        var response = await _frecuencyTypeUnitOfWork.GetAsync(pagination);
        if (!response.WasSuccess)
        {
            return BadRequest(response.Message);
        }
        return Ok(response.Result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var response = await _frecuencyTypeUnitOfWork.GetAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPut]
    public async Task<ActionResult<FrecuencyType>> PutAsync(FrecuencyType modelo)
    {
        var response = await _frecuencyTypeUnitOfWork.UpdateAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpPost]
    public async Task<ActionResult<FrecuencyType>> PostAsync(FrecuencyType modelo)
    {
        var response = await _frecuencyTypeUnitOfWork.AddAsync(modelo);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync(int id)
    {
        var response = await _frecuencyTypeUnitOfWork.DeleteAsync(id);
        if (response.WasSuccess)
        {
            return Ok(response.Result);
        }
        return NotFound(response.Message);
    }
}