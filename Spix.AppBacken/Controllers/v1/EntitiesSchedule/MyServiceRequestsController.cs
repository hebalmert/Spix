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

namespace Spix.AppBacken.Controllers.v1.EntitiesSchedule;

//El portal del CLIENTE, en su propio controlador. Solo el rol Client entra aqui, y lo que
//devuelve son DTOs recortados: asi no hay forma de que se le escape un dato de la oficina.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/myservicerequests")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client")]
[ApiController]
public class MyServiceRequestsController : ControllerBase
{
    private readonly IMyServiceRequestServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public MyServiceRequestsController(IMyServiceRequestServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    //Mis solicitudes
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Mis contratos, para elegir sobre cual pido la visita
    [HttpGet("contracts")]
    public async Task<IActionResult> GetMyContractsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetMyContractsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Pedir una visita
    [HttpPost]
    public async Task<IActionResult> PostAsync(MyServiceRequestDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
