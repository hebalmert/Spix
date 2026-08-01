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
