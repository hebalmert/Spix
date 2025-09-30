using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ResponcesSec;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.AppBack.Controllers.EntitiesGenV1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/zones")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Usuario")]
[ApiController]
public class ZonesController : ControllerBase
{
    private readonly IZoneUnitOfWork _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ZonesController(IZoneUnitOfWork unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet("loadCombo/{id:int}")]
    public async Task<IActionResult> GetComboAsync(int id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.ComboAsync(userClaimsInfo.UserName, id);
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
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
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
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

    [HttpPut]
    public async Task<IActionResult> PutAsync(Zone modelo)
    {
        try
        {
            var response = await _unitOfWork.UpdateAsync(modelo);
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
    public async Task<IActionResult> PostAsync(Zone modelo)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
            var response = await _unitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
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
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
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
}