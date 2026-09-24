using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesBilling;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBacken.Controllers.v1.EntitiesBilling;

//Las facturas del CLIENTE, en su propio controlador. Solo entra el rol Client y lo que
//devuelve son DTOs recortados; el servicio ademas cruza todo contra su propio ClientId.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mybills")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client")]
[ApiController]
public class MyBillsController : ControllerBase
{
    private readonly IMyBillServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public MyBillsController(IMyBillServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    //Mis facturas, paginadas
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que debo hoy: el numerito de la tarjeta del portal
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //El detalle de una factura mia
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
