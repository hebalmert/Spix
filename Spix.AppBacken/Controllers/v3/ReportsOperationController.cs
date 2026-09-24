using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.v3;

//Los reportes de la operacion, en la version de reportes
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/reports-operation")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ReportsOperationController : ControllerBase
{
    private readonly IReportOperationServiceX _reportOperationService;
    private readonly IStringLocalizer _localizer;

    public ReportsOperationController(IReportOperationServiceX reportOperationService, IStringLocalizer localizer)
    {
        _reportOperationService = reportOperationService;
        _localizer = localizer;
    }

    //Los contratos que entraron en el periodo
    [HttpGet("contracts/summary")]
    public async Task<IActionResult> GetContractsSummaryAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetContractsSummaryAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los planes que mas se contrataron
    [HttpGet("contracts/top-plans")]
    public async Task<IActionResult> GetTopPlansAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetTopPlansAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que paso con las solicitudes del periodo
    [HttpGet("services/summary")]
    public async Task<IActionResult> GetServicesSummaryAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetServicesSummaryAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los servicios que mas se repiten
    [HttpGet("services/top")]
    public async Task<IActionResult> GetTopServicesAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetTopServicesAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los tecnicos que mas resolvieron
    [HttpGet("services/technicians")]
    public async Task<IActionResult> GetTopTechniciansAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetTopTechniciansAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Como le fue al corte del periodo
    [HttpGet("cutoff/summary")]
    public async Task<IActionResult> GetCutOffSummaryAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetCutOffSummaryAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Donde se corto mas y donde respondieron mejor
    [HttpGet("cutoff/zones")]
    public async Task<IActionResult> GetCutOffZonesAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetCutOffZonesAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los contratos que hoy no estan dando servicio
    [HttpGet("churn/summary")]
    public async Task<IActionResult> GetChurnSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetChurnSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //En que zonas se esta yendo la gente
    [HttpGet("churn/zones")]
    public async Task<IActionResult> GetChurnZonesAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportOperationService.GetChurnZonesAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
