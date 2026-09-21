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
[Route("api/v{version:apiVersion}/contractaudit")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class ContractAuditController : ControllerBase
{
    private readonly IContractAuditServiceX _contractAuditService;
    private readonly IStringLocalizer _localizer;

    public ContractAuditController(
        IContractAuditServiceX contractAuditService,
        IStringLocalizer localizer)
    {
        _contractAuditService = contractAuditService;
        _localizer = localizer;
    }

    //La linea de tiempo completa de un contrato
    [HttpGet("{contractClientId}")]
    public async Task<IActionResult> GetAsync(Guid contractClientId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractAuditService.GetAsync(contractClientId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
