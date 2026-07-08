using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSignature;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.Pagination;

namespace Spix.AppBack.Controllers.EntitiesContracts;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contractdocuments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar, Client")]
[ApiController]
public class ContractDocumentsController : ControllerBase
{
    private readonly ISignatureServiceX _signatureService;
    private readonly IStringLocalizer _localizer;

    public ContractDocumentsController(ISignatureServiceX signatureService, IStringLocalizer localizer)
    {
        _signatureService = signatureService;
        _localizer = localizer;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplatesAsync([FromQuery] PaginationDTO pagination)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.GetTemplatesAsync(pagination, userClaimsInfo.UserName);
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

    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetTemplateAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.GetTemplateAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("templates")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> PostTemplateAsync(ContractDocumentTemplate model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.AddTemplateAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPut("templates")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> PutTemplateAsync(ContractDocumentTemplate model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.UpdateTemplateAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("templates/test/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> TestTemplateAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.TestTemplateAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("templates/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> DeleteTemplateAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.DeleteTemplateAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("fields")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> PostFieldAsync(ContractDocumentTemplateField model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.AddTemplateFieldAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpDelete("fields/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> DeleteFieldAsync(Guid id)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.DeleteTemplateFieldAsync(id, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpGet("contract/{contractClientId}")]
    public async Task<IActionResult> GetContractDocumentsAsync(Guid contractClientId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.GetContractDocumentsAsync(contractClientId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("generate/{contractClientId}/{templateId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> GenerateAsync(Guid contractClientId, Guid templateId)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.GenerateContractDocumentAsync(contractClientId, templateId, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("generate/{contractClientId}/type/{documentType}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    public async Task<IActionResult> GenerateByTypeAsync(Guid contractClientId, ContractDocumentType documentType)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.GenerateContractDocumentByTypeAsync(contractClientId, documentType, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }

    [HttpPost("sign")]
    public async Task<IActionResult> SignAsync(ContractSignedDocument model)
    {
        ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
        var response = await _signatureService.SignContractDocumentAsync(model, userClaimsInfo.UserName);
        return ResponseHelper.Format(response);
    }
}
