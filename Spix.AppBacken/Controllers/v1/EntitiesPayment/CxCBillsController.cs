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
[Route("api/v{version:apiVersion}/cxcbills")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Technician")]
[ApiController]
public class CxCBillsController : ControllerBase
{
    private readonly IPaymentServiceX _paymentService;
    private readonly IStringLocalizer _localizer;

    public CxCBillsController(IPaymentServiceX paymentService, IStringLocalizer localizer)
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
            var response = await _paymentService.GetCxCBillsAsync(pagination, userClaimsInfo.UserName);
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
        var response = await _paymentService.GetCxCBillAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("pay")]
    public async Task<IActionResult> PayAsync(CxCBillPaymentDto model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.PayCxCBillAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelAsync(CxCBillCancelDto model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _paymentService.CancelCxCBillAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
