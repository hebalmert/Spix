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
[Route("api/v{version:apiVersion}/contractsuspendedaudits")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractSuspendedAuditController : ControllerBase
{
    private readonly IContractSuspendedAuditServiceX _contractSuspendedAuditService;
    private readonly IStringLocalizer _localizer;

    public ContractSuspendedAuditController(
        IContractSuspendedAuditServiceX contractSuspendedAuditService,
        IStringLocalizer localizer)
    {
        _contractSuspendedAuditService = contractSuspendedAuditService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractSuspendedAuditService.GetAsync(
            startDate,
            endDate,
            userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
