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
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination, [FromQuery] int? status)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetAsync(pagination, status, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los numeros del tablero
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.GetSummaryAsync(userClaimsInfo.UserName);
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

    //Registrar la visita es de la oficina, no del tecnico
    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
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

    //Agendar la solicitud del cliente: tecnico y fecha
    [HttpPost("{id}/assign")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> AssignAsync(Guid id, [FromQuery] Guid technicianId, [FromQuery] DateTime scheduledAtUtc)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.AssignAsync(id, technicianId, scheduledAtUtc, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Cerrar la visita: exige servicio, comentario y foto del despues
    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseAsync(Guid id, [FromQuery] string? comment, [FromQuery] string? recommendation)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.CloseAsync(id, comment, recommendation, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Resuelta por telefono: se cierra sin mandar a nadie
    [HttpPost("{id}/resolvebyphone")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> ResolveByPhoneAsync(Guid id, [FromQuery] string? comment, [FromQuery] string? recommendation)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.ResolveByPhoneAsync(id, comment, recommendation, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Eliminar la orden tampoco es del tecnico
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _unitOfWork.DeleteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
