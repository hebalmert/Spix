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
[Route("api/v{version:apiVersion}/contractexempt")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractExemptController : ControllerBase
{
    private readonly IContractExemptServiceX _contractExemptService;
    private readonly IStringLocalizer _localizer;

    public ContractExemptController(
        IContractExemptServiceX contractExemptService,
        IStringLocalizer localizer)
    {
        _contractExemptService = contractExemptService;
        _localizer = localizer;
    }

    //Exonerar de forma permanente: reglas propias de este modulo
    [HttpPost("{id}/exempt")]
    public async Task<IActionResult> ExemptAsync(Guid id, [FromQuery] string? motivo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractExemptService.ExemptAsync(id, motivo, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Listado del registro de exoneraciones, con sus totales
    [HttpGet("records")]
    public async Task<IActionResult> GetRecordsAsync([FromQuery] string? filter, [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta, [FromQuery] bool soloAbiertas = true)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractExemptService.GetRecordsAsync(filter, desde, hasta, soloAbiertas, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Contratos activos que se pueden exonerar (autocompletar del Create)
    [HttpGet("active")]
    public async Task<IActionResult> SearchActiveAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractExemptService.SearchActiveAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Retirar la exoneracion: el contrato vuelve a Activo
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractExemptService.ActivateAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
