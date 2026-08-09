using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesSaaSDTO;

namespace Spix.AppBacken.Controllers.v1.EntitiesSaaS;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/saas-subscriptions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[AllowExpiredSubscriptionAccess]
[ApiController]
public class SaasSubscriptionsController : ControllerBase
{
    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;
    private readonly IStringLocalizer _localizer;

    public SaasSubscriptionsController(ISaasSubscriptionServiceX saasSubscriptionService, IStringLocalizer localizer)
    {
        _saasSubscriptionService = saasSubscriptionService;
        _localizer = localizer;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatusAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _saasSubscriptionService.GetAccessAsync(userClaimsInfo.CorporationId);
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

    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutAsync(SubscriptionCheckoutRequestDTO request)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _saasSubscriptionService.CreateCheckoutAsync(userClaimsInfo.CorporationId,
                userClaimsInfo.UserName, request);
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
