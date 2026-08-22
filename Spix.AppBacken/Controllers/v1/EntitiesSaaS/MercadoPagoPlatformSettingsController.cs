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
[Route("api/v{version:apiVersion}/mercadopagosettings")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class MercadoPagoPlatformSettingsController : ControllerBase
{
    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;
    private readonly IStringLocalizer _localizer;

    public MercadoPagoPlatformSettingsController(ISaasSubscriptionServiceX saasSubscriptionService,
        IStringLocalizer localizer)
    {
        _saasSubscriptionService = saasSubscriptionService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var response = await _saasSubscriptionService.GetMercadoPagoSettingAsync();
            return ResponseHelper.Format(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(MercadoPagoPlatformSettingDTO setting)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _saasSubscriptionService.SaveMercadoPagoSettingAsync(setting, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }
}
