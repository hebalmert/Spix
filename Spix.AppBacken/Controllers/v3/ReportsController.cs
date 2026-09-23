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

//Los reportes viven aparte, en su propia version del API (api/v3/...).
//
//Asi una consulta pesada de reportes nunca se mezcla con los endpoints que el sistema usa
//todo el dia: se pueden limitar, cachear o mover de servidor sin tocar nada de v1.
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/reports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportServiceX _reportService;
    private readonly IStringLocalizer _localizer;

    public ReportsController(IReportServiceX reportService, IStringLocalizer localizer)
    {
        _reportService = reportService;
        _localizer = localizer;
    }

    //Los totales de los contratos activos
    [HttpGet("active/summary")]
    public async Task<IActionResult> GetActiveSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetActiveSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los contratos activos con su plan y su monto, paginados
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveContractsAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetActiveContractsAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los combos del reporte por zona: estado, ciudad y zona
    [HttpGet("combostates")]
    public async Task<IActionResult> ComboStatesAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.ComboStatesAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("combocities/{stateId:int}")]
    public async Task<IActionResult> ComboCitiesAsync(int stateId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.ComboCitiesAsync(stateId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("combozones/{cityId:int}")]
    public async Task<IActionResult> ComboZonesAsync(int cityId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.ComboZonesAsync(cityId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que factura una zona y sus contratos
    [HttpGet("by-zone/{zoneId:guid}/summary")]
    public async Task<IActionResult> GetZoneSummaryAsync(Guid zoneId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetZoneSummaryAsync(zoneId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("by-zone/{zoneId:guid}")]
    public async Task<IActionResult> GetZoneContractsAsync(Guid zoneId, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetZoneContractsAsync(zoneId, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los combos de los reportes por AP y por servidor
    [HttpGet("combonodes")]
    public async Task<IActionResult> ComboNodesAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.ComboNodesAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("comboservers")]
    public async Task<IActionResult> ComboServersAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.ComboServersAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que genera un AP y sus contratos
    [HttpGet("by-node/{nodeId:guid}/summary")]
    public async Task<IActionResult> GetNodeSummaryAsync(Guid nodeId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetNodeSummaryAsync(nodeId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("by-node/{nodeId:guid}")]
    public async Task<IActionResult> GetNodeContractsAsync(Guid nodeId, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetNodeContractsAsync(nodeId, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que genera un servidor y sus contratos
    [HttpGet("by-server/{serverId:guid}/summary")]
    public async Task<IActionResult> GetServerSummaryAsync(Guid serverId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetServerSummaryAsync(serverId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("by-server/{serverId:guid}")]
    public async Task<IActionResult> GetServerContractsAsync(Guid serverId, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportService.GetServerContractsAsync(serverId, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
