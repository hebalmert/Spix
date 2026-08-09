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
[Route("api/v{version:apiVersion}/system-settings")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingServiceX _unitOfWork;
    private readonly IStringLocalizer _localizer;

    public SystemSettingsController(
        ISystemSettingServiceX unitOfWork,
        IStringLocalizer localizer)
    {
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            _ = User.GetSecurityContextOrThrow(_localizer, HttpContext);

            var response = await _unitOfWork.GetSystemAsync();

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (Exception exception)
        {
            return StatusCode(
                500,
                _localizer["Generic_UnexpectedError"] + ": " + exception.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveAsync(List<SecretSettingDTO> items)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

            var response = await _unitOfWork.SaveSystemAsync(items, userClaimsInfo.UserName);

            return ResponseHelper.Format(response);
        }
        catch (ApplicationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (Exception exception)
        {
            return StatusCode(
                500,
                _localizer["Generic_UnexpectedError"] + ": " + exception.Message);
        }
    }
}
