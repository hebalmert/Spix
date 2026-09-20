using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfaceContratos;
using Spix.DomainLogic.AppResponses;

namespace Spix.AppBack.Controllers.v1.EntitiesContracts;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contractsuspended")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractSuspendedController : ControllerBase
{
    private readonly IContractSuspendedServiceX _contractSuspendedService;
    private readonly IStringLocalizer _localizer;

    public ContractSuspendedController(
        IContractSuspendedServiceX contractSuspendedService,
        IStringLocalizer localizer)
    {
        _contractSuspendedService = contractSuspendedService;
        _localizer = localizer;
    }

    //Suspender: reglas propias de este modulo
    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendAsync(Guid id, [FromQuery] string? motivo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedService.SuspendAsync(id, motivo, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Listado del registro de suspensiones, con sus totales
    [HttpGet("records")]
    public async Task<IActionResult> GetRecordsAsync([FromQuery] string? filter, [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta, [FromQuery] bool soloAbiertas = true)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedService.GetRecordsAsync(filter, desde, hasta, soloAbiertas, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Contratos activos que se pueden suspender (autocompletar del Create)
    [HttpGet("active")]
    public async Task<IActionResult> SearchActiveAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedService.SearchActiveAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedService.SearchAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedService.ActivateAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
