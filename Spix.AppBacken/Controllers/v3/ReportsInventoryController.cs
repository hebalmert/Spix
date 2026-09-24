using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesInven;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.v3;

//El reporte de los seriales, en la version de reportes: no toca los modulos de inventario
[ApiVersion("3.0")]
[Route("api/v{version:apiVersion}/reports-inventory")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ReportsInventoryController : ControllerBase
{
    private readonly IReportInventoryServiceX _reportInventoryService;
    private readonly IStringLocalizer _localizer;

    public ReportsInventoryController(IReportInventoryServiceX reportInventoryService, IStringLocalizer localizer)
    {
        _reportInventoryService = reportInventoryService;
        _localizer = localizer;
    }

    //Los totales de seriales de la corporacion
    [HttpGet("serials/summary")]
    public async Task<IActionResult> GetSerialSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Una fila por producto
    [HttpGet("serials")]
    public async Task<IActionResult> GetSerialsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _reportInventoryService.GetSerialsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
