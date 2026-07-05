using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesDashboard;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.Dashboard;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrator, Auxiliar, Cachier, Collector, Contractor, WarehouseLead")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardServiceX _dashboardService;
    private readonly IStringLocalizer _localizer;

    public DashboardController(IDashboardServiceX dashboardService, IStringLocalizer localizer)
    {
        _dashboardService = dashboardService;
        _localizer = localizer;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _dashboardService.GetSummaryAsync(userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"] + ": " + ex.Message);
        }
    }
}
