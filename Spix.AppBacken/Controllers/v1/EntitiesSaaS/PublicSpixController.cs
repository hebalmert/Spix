using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.EntitiesSaaSDTO;

namespace Spix.AppBacken.Controllers.v1.EntitiesSaaS;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/public-spix")]
[AllowAnonymous]
[ApiController]
public class PublicSpixController : ControllerBase
{
    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;
    private readonly IStringLocalizer _localizer;

    public PublicSpixController(ISaasSubscriptionServiceX saasSubscriptionService, IStringLocalizer localizer)
    {
        _saasSubscriptionService = saasSubscriptionService;
        _localizer = localizer;
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlansAsync()
    {
        try
        {
            var response = await _saasSubscriptionService.GetPublicPlansAsync();
            return ResponseHelper.Format(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpPost("trial")]
    public async Task<IActionResult> StartTrialAsync(StartTrialRequestDTO request)
    {
        try
        {
            string frontUrl = Request.Headers.Origin.FirstOrDefault()
                ?? HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var response = await _saasSubscriptionService.StartTrialAsync(request, frontUrl);
            return ResponseHelper.Format(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }
}
