using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.Entities;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/frecuencies")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class FrecuenciesController : ControllerBase
{
    private readonly IFrecuencyServiceX _frecuencyUnitOfWork;

    public FrecuenciesController(IFrecuencyServiceX frecuencyUnitOfWork)
    {
        _frecuencyUnitOfWork = frecuencyUnitOfWork;
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