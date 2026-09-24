using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesPayment;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.v3;

//Los reportes del dinero, en la version de reportes: no tocan los modulos de cobro
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/reports-finance")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ReportsFinanceController : ControllerBase
{
    private readonly IReportFinanceServiceX _reportFinanceService;
    private readonly IStringLocalizer _localizer;

    public ReportsFinanceController(IReportFinanceServiceX reportFinanceService, IStringLocalizer localizer)
    {
        _reportFinanceService = reportFinanceService;
        _localizer = localizer;
    }

    //Cuanto entro en el periodo y como
    [HttpGet("collections/summary")]
    public async Task<IActionResult> GetCollectionSummaryAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetCollectionSummaryAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Quien recogio y cuanto
    [HttpGet("collections/collectors")]
    public async Task<IActionResult> GetCollectorsAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetCollectorsAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Las notas emitidas en el periodo
    [HttpGet("notes/summary")]
    public async Task<IActionResult> GetNotesSummaryAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetNotesSummaryAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //La cartera viva repartida por antiguedad
    [HttpGet("aging/summary")]
    public async Task<IActionResult> GetAgingAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetAgingAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los contratos que mas deben
    [HttpGet("aging/debtors")]
    public async Task<IActionResult> GetTopDebtorsAsync([FromQuery] int top = 20)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetTopDebtorsAsync(top, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Lo que se le causo a cada contratista en el periodo
    [HttpGet("contractors")]
    public async Task<IActionResult> GetContractorCommissionsAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetContractorCommissionsAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //La bitacora del dinero, paginada
    [HttpGet("audit")]
    public async Task<IActionResult> GetAuditAsync([FromQuery] PaginationDTO pagination, [FromQuery] int eventType = 0)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.GetAuditAsync(eventType, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los tipos de movimiento de la bitacora
    [HttpGet("audit/comboevents")]
    public async Task<IActionResult> ComboEventTypesAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportFinanceService.ComboEventTypesAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
