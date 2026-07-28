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
[Route("api/v{version:apiVersion}/contractor-payments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[ApiController]
public class ContractorPaymentsController : ControllerBase
{
    private readonly IContractorPaymentServiceX _contractorPaymentService;
    private readonly IStringLocalizer _localizer;

    public ContractorPaymentsController(
        IContractorPaymentServiceX contractorPaymentService,
        IStringLocalizer localizer)
    {
        _contractorPaymentService = contractorPaymentService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _contractorPaymentService.GetAccountPayablesAsync(pagination, userClaimsInfo.UserName);
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

    [HttpPost("pay")]
    public async Task<IActionResult> PayAsync(ContractorPaymentCreateDto model)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _contractorPaymentService.PayAsync(model, userClaimsInfo.UserName);
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
