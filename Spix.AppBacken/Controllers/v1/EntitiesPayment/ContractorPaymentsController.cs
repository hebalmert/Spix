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

    //Los numeros del tablero
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetCxCSummaryAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los contratistas que tienen comisiones pendientes
    [HttpGet("combocontractors")]
    public async Task<IActionResult> ComboContractorsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.ComboContractorsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Las comisiones pendientes de un contratista
    [HttpGet("pending/{contractorId:guid}")]
    public async Task<IActionResult> GetPendingAsync(Guid contractorId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetPendingAsync(contractorId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Arma la cuenta por pagar del contratista
    [HttpPost("cxc")]
    public async Task<IActionResult> CreateCxCAsync(CxCContractorCreateDto model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.CreateCxCContractorAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Las cuentas por pagar, paginadas
    [HttpGet("cxc")]
    public async Task<IActionResult> GetCxCAsync([FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetCxCContractorsAsync(pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Una cuenta con lo que la compone y lo que se le ha pagado
    [HttpGet("cxc/{id:guid}")]
    public async Task<IActionResult> GetCxCByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetCxCContractorAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Las comisiones que componen la cuenta, paginadas
    [HttpGet("cxc/{id:guid}/commissions")]
    public async Task<IActionResult> GetCommissionsAsync(Guid id, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetCommissionsAsync(id, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Los abonos de la cuenta, paginados
    [HttpGet("cxc/{id:guid}/payments")]
    public async Task<IActionResult> GetPaymentsAsync(Guid id, [FromQuery] PaginationDTO pagination)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.GetPaymentsAsync(id, pagination, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Le paga al contratista: completo o por partes
    [HttpPost("cxc/pay")]
    public async Task<IActionResult> PayCxCAsync(CxCContractorPaymentDto model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.PayCxCContractorAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Anula la cuenta y devuelve sus comisiones a pendientes
    [HttpPost("cxc/{id:guid}/cancel")]
    public async Task<IActionResult> CancelCxCAsync(Guid id, [FromBody] string motivo)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _contractorPaymentService.CancelCxCContractorAsync(id, motivo, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
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
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
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
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }
}
