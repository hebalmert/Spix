using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.Entities;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/managers")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class ManagersController : ControllerBase
{
    private readonly IManagerServiceX _managerUnitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer _localizer;

    public ManagersController(IManagerServiceX managerUnitOfWork, IConfiguration configuration,
        IStringLocalizer localizer)
    {
        _managerUnitOfWork = managerUnitOfWork;
        _configuration = configuration;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination)
    {
        try
        {
            var response = await _managerUnitOfWork.GetAsync(pagination);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        try
        {
            var response = await _managerUnitOfWork.GetAsync(id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(Manager modelo)
    {
        try
        {
            var response = await _managerUnitOfWork.UpdateAsync(modelo, _configuration["UrlFrontend"]!);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(Manager modelo)
    {
        try
        {
            var response = await _managerUnitOfWork.AddAsync(modelo, _configuration["UrlFrontend"]!);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        try
        {
            var response = await _managerUnitOfWork.DeleteAsync(id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message); // Ya está localizado
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }
}