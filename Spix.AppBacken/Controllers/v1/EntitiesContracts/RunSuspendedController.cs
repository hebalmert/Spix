using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.v1.EntitiesContracts;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/runsuspended")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class RunSuspendedController : ControllerBase
{
    private readonly IRunSuspendedServiceX _runSuspendedService;
    private readonly IStringLocalizer _localizer;

    public RunSuspendedController(
        IRunSuspendedServiceX runSuspendedService,
        IStringLocalizer localizer)
    {
        _runSuspendedService = runSuspendedService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.GetAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("combomonths")]
    public async Task<IActionResult> ComboMonthsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.ComboMonthsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.GetByIdAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(RunSuspended model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.AddAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("{id}/run")]
    public async Task<IActionResult> RunAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.RunAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(RunSuspended model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.UpdateAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _runSuspendedService.DeleteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
