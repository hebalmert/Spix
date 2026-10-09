using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBacken.Controllers.v4.EntitiesSchedule;

//La puerta de la app del tecnico. Version propia siguiendo el esquema: v1 web, v2
//escritorio, v3 reportes, v4 app. Asi un cambio para la oficina no rompe lo que esta
//instalado en los telefonos de la calle.
[ApiVersion("4.0")]
[Route("api/v{version:apiVersion}/techvisits")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Technician")]
[ApiController]
public class TechVisitsController : ControllerBase
{
    private readonly ITechVisitServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public TechVisitsController(ITechVisitServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    //La jornada: lo abierto y lo que cerro hoy
    [HttpGet]
    public async Task<IActionResult> GetMineAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetMineAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Llegue y empiezo
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.StartAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Donde estoy. Se puede mandar varias veces: la ultima manda
    [HttpPost("{id}/capturelocation")]
    public async Task<IActionResult> CaptureLocationAsync(Guid id, [FromQuery] decimal latitude, [FromQuery] decimal longitude)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.CaptureLocationAsync(id, latitude, longitude, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseAsync(Guid id, [FromQuery] string? comment, [FromQuery] string? recommendation)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.CloseAsync(id, comment, recommendation, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //La foto del trabajo. Viaja en base64, igual que desde el navegador
    [HttpPost("photo")]
    public async Task<IActionResult> AddPhotoAsync(ServiceRequestPhotoDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddPhotoAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //El servicio realizado: sin esto la visita no se puede cerrar
    [HttpPost("detail")]
    public async Task<IActionResult> AddDetailAsync(ServiceRequestDetailDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddDetailAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("detail/{id}")]
    public async Task<IActionResult> DeleteDetailAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteDetailAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Fui y no habia nadie: exige la coordenada de donde estuve
    [HttpPost("{id}/noclient")]
    public async Task<IActionResult> NoClientAsync(Guid id, [FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] string? comment)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.NoClientAsync(id, latitude, longitude, comment, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
