using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesBilling;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesBilling;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billingnoteones")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class BillingNoteOnesController : ControllerBase
{
    private readonly IBillingServiceX _billingService;
    private readonly IStringLocalizer _localizer;

    public BillingNoteOnesController(IBillingServiceX billingService, IStringLocalizer localizer)
    {
        _billingService = billingService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
            var response = await _billingService.GetBillingNoteOnesAsync(pagination, userClaimsInfo.UserName);
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
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.ComboMonthsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("searchcontracts")]
    public async Task<IActionResult> SearchContractsAsync([FromQuery] string filter)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.SearchContractsAsync(filter, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.GetBillingNoteOneAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(BillingNoteOne model)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.AddBillingNoteOneAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(BillingNoteOne model)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.UpdateBillingNoteOneAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
        var response = await _billingService.DeleteBillingNoteOneAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
