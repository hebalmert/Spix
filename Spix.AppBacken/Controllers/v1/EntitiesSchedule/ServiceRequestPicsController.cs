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

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servicerequestpics")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Technician")]
[ApiController]
public class ServiceRequestPicsController : ControllerBase
{
    private readonly IServiceRequestPicServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestPicsController(IServiceRequestPicServiceX unitOfWork, IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet("byrequest/{serviceRequestId}")]
    public async Task<IActionResult> GetByServiceRequestAsync(Guid serviceRequestId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetByServiceRequestAsync(serviceRequestId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(ServiceRequestPic modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.UpdateAsync(modelo, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(ServiceRequestPic modelo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddAsync(modelo, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
