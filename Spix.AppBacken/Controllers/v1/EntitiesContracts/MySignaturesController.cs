using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.AppInfra.ErrorHandling;
using Spix.AppServiceX.InterfacesSignature;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppBack.Controllers.EntitiesContracts;

//Portal del cliente: sus propios documentos para firmar.
//Cada accion se resuelve con el usuario del token, nunca con un id que mande el navegador.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mysignatures")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client")]
[ApiController]
public class MySignaturesController : ControllerBase
{
    private readonly ISignatureServiceX _signatureService;
    private readonly IStringLocalizer _localizer;
    private readonly IConfiguration _configuration;

    public MySignaturesController(ISignatureServiceX signatureService, IStringLocalizer localizer,
        IConfiguration configuration)
    {
        _signatureService = signatureService;
        _localizer = localizer;
        _configuration = configuration;
    }

    //Aviso de firma electronica que debe aceptar antes de firmar
    [HttpGet("terms")]
    public IActionResult GetTerms() =>
        Ok(new SignatureTermsDTO
        {
            Version = ElectronicSignatureConsent.Version,
            Text = ElectronicSignatureConsent.Text
        });

    //Enlace del PDF: se pide al abrir el documento y dura pocos minutos
    [HttpGet("link/{contractClientId}/{documentType}")]
    public async Task<IActionResult> GetLinkAsync(Guid contractClientId, ContractDocumentType documentType)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.GetMyDocumentLinkAsync(contractClientId, documentType, userClaimsInfo);
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

    [HttpGet]
    public async Task<IActionResult> GetMyDocumentsAsync()
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.GetMyDocumentsAsync(userClaimsInfo.UserName);
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

    //Pide el codigo de un solo uso; se envia al correo registrado del cliente
    [HttpPost("code/{contractClientId}/{documentType}")]
    public async Task<IActionResult> RequestCodeAsync(Guid contractClientId, ContractDocumentType documentType)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.RequestSignatureCodeAsync(contractClientId, documentType, userClaimsInfo);
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

    [HttpPost("sign")]
    public async Task<IActionResult> SignAsync(SignDocumentRequestDTO model)
    {
        try
        {
            ClaimsDTOs userClaimsInfo = User.GetSecurityContextOrThrow(_localizer, HttpContext);
            var response = await _signatureService.SignMyDocumentAsync(model, _configuration["UrlFrontend"] ?? string.Empty, userClaimsInfo);
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
}
