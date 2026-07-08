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
using Spix.DomainLogic.Pagination;

namespace Spix.AppBacken.Controllers.v1.EntitiesSchedule;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/servicerequests")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Technician")]
[ApiController]
public class ServiceRequestsController : ControllerBase
{
    private readonly IServiceRequestServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestsController(IServiceRequestServiceX unitOfWork, IStringLocalizer localizer)
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

    [HttpGet("searchcontracts")]
    public async Task<IActionResult> SearchContractsAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.SearchContractsAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(ServiceRequestDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(ServiceRequestDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.UpdateAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("details")]
    public async Task<IActionResult> PostDetailAsync(ServiceRequestDetailDto dto)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AddDetailAsync(dto, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("details/{id}")]
    public async Task<IActionResult> DeleteDetailAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteDetailAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
