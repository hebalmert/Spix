using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBacken.Controllers.v1.EntitiesSchedule;

//La bandeja es de la oficina: el tecnico no revisa su propio trabajo
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/visitreviews")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class VisitReviewsController : ControllerBase
{
    private readonly IVisitReviewServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public VisitReviewsController(IVisitReviewServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    //El contador del menu y de las pestañas
    [HttpGet("counters")]
    public async Task<IActionResult> GetCountersAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetCountersAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Pestaña Ubicacion: marco lejos del sitio y el cliente si estaba
    [HttpGet("location")]
    public async Task<IActionResult> GetLocationAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetLocationAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Pestaña Sin cliente
    [HttpGet("absent")]
    public async Task<IActionResult> GetAbsentAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAbsentAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Aplicar la coordenada del tecnico a la ubicacion del contrato
    [HttpPost("{id}/applylocation")]
    public async Task<IActionResult> ApplyLocationAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.ApplyLocationAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Dejar la ubicacion como esta y sacar la visita de la bandeja
    [HttpPost("{id}/dismiss")]
    public async Task<IActionResult> DismissAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DismissAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
