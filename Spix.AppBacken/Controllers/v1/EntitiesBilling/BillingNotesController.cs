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
[Route("api/v{version:apiVersion}/billingnotes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
[ApiController]
public class BillingNotesController : ControllerBase
{
    private readonly IBillingServiceX _billingService;
    private readonly IStringLocalizer _localizer;

    public BillingNotesController(IBillingServiceX billingService, IStringLocalizer localizer)
    {
        _billingService = billingService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _billingService.GetBillingNotesAsync(pagination, userClaimsInfo.UserName);
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

    [HttpGet("combomonths")]
    public async Task<IActionResult> ComboMonthsAsync()
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.ComboMonthsAsync(userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.GetBillingNoteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync(BillingNote model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.AddBillingNoteAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    //Revision previa: que contratos activos estan incompletos antes de lanzar
    [HttpGet("{id}/check")]
    public async Task<IActionResult> CheckAsync(Guid id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);

            var note = await _billingService.GetBillingNoteAsync(id, userClaimsInfo.UserName);
            if (!note.WasSuccess || note.Result == null)
                return ResponseHelper.Format(note);

            var response = await _billingService.CheckContractsAsync(note.Result.YearNumber, note.Result.MonthType, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    //Un lote de contratos: el front lo llama tantas veces como haga falta y va mostrando el avance
    [HttpPost("{id}/launch/batch")]
    public async Task<IActionResult> LaunchBatchAsync(Guid id, [FromBody] List<Guid> contractClientIds)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _billingService.LaunchBatchAsync(id, contractClientIds, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    //Cierra la nota cuando ya se recorrieron todos los lotes
    [HttpPost("{id}/launch/finish")]
    public async Task<IActionResult> FinishLaunchAsync(Guid id)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _billingService.FinishLaunchAsync(id, userClaimsInfo.UserName);
            return ResponseHelper.Format(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, _localizer["Generic_UnexpectedError"].Value);
        }
    }

    [HttpPost("{id}/launch")]
    public async Task<IActionResult> LaunchAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.LaunchBillingNoteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync(BillingNote model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.UpdateBillingNoteAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _billingService.DeleteBillingNoteAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
