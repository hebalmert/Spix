using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesPayment;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/preexonerateds")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class PreExoneratedsController : ControllerBase
{
    private readonly IPaymentServiceX _paymentService;
    private readonly IStringLocalizer _localizer;

    public PreExoneratedsController(IPaymentServiceX paymentService, IStringLocalizer localizer)
    {
        _paymentService = paymentService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _paymentService.GetPreExoneratedsAsync(pagination, userClaimsInfo.UserName);
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

    [HttpGet("combomonths")]
    public async Task<IActionResult> ComboMonthsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.ComboMonthsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("searchcontracts")]
    public async Task<IActionResult> SearchContractsAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.SearchContractsAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.GetPreExoneratedAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(PreExonerated model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.AddPreExoneratedAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(PreExonerated model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.UpdatePreExoneratedAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.DeletePreExoneratedAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
