using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppBacken.Controllers.v1.EntitiesGen;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/estratossociales")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class EstratosSocialesController : ControllerBase
{
    private readonly IEstratoSocialServiceX _estratoSocialServiceX;
    private readonly IStringLocalizer _localizer;

    public EstratosSocialesController(
        IEstratoSocialServiceX estratoSocialServiceX,
        IStringLocalizer localizer)
    {
        _estratoSocialServiceX = estratoSocialServiceX;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _estratoSocialServiceX.GetAsync(pagination, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        try
        {
            var response = await _estratoSocialServiceX.GetAsync(id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }

    [HttpGet("loadCombo")]
    public async Task<IActionResult> LoadComboAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _estratoSocialServiceX.ComboAsync(userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(EstratoSocial modelo)
    {
        try
        {
            var response = await _estratoSocialServiceX.UpdateAsync(modelo);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(EstratoSocial modelo)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _estratoSocialServiceX.AddAsync(modelo, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var response = await _estratoSocialServiceX.DeleteAsync(id);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, _localizer[nameof(Resource.Generic_UnexpectedError)].Value);
        }
    }
}
