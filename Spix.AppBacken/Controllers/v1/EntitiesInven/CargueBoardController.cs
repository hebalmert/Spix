using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesInven;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesInven;

//Solo lectura del cargue de seriales: el tablero, el avance y donde quedo cada equipo.
//En su propio controlador para no tocar lo que ya devuelven cargues y cargueDetails,
//que usan otras pantallas.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cargueboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class CargueBoardController : ControllerBase
{
    private readonly ICargueBoardServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public CargueBoardController(ICargueBoardServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}/progress")]
    public async Task<IActionResult> GetProgressAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetProgressAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}/serials")]
    public async Task<IActionResult> GetSerialsAsync(Guid id, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetSerialsAsync(id, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
