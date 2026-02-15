using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceEntities;
using Spix.Domain.Entities;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.Entities;

[ApiController]
[ApiVersion("2.0")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[Route("api/v{version:apiVersion}/states")]
public class StatesController : ControllerBase
{
    private readonly IStateServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public StatesController(IStateServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            //ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.GetAsync(pagination);
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
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            //UserClaimsInfo userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.GetAsync(id);
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
    public async Task<IActionResult> Post([FromBody] State model)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.AddAsync(model);
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
    public async Task<IActionResult> Put([FromBody] State model)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            //UserClaimsInfo userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.UpdateAsync(model);
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
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.DeleteAsync(id);
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

    [HttpGet("loadCombo")]
    public async Task<IActionResult> GetCombo(int id)
    {
        try
        {
            //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _unitOfWork.ComboAsync(userClaimsInfo);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}